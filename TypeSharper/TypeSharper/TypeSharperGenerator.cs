using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using TypeSharper.Diagnostics;
using TypeSharper.Generator;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Type;
using TypeSharper.SemanticExtensions;

namespace TypeSharper;

[Generator]
public class TypeSharperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        InitializeTypeGenerators(context);

        RegisterTypeGeneratorAttributes(context);

        context.RegisterSourceOutput(
            TsTargetTypes(context),
            (sourceProductionContext, t) =>
            {
                var (model, targets, firstErrorSymbol) = t;

                if (!RunDiagnostics(model, targets, firstErrorSymbol, sourceProductionContext))
                {
                    return;
                }

                var maybeGeneratedModel = RunGenerators(model, targets, sourceProductionContext);

                maybeGeneratedModel.IfSome(
                    generatedModel =>
                    {
                        var diffModel = model.Diff(generatedModel);
                        var syntaxTreesWithPaths =
                            diffModel
                                .Types
                                .Where(type => type.IsTopLevel() && type.TypeKind != TsType.EKind.Special)
                                .Select(
                                    newType =>
                                    {
                                        var syntaxTree = CSharpSyntaxTree.ParseText(newType.Cs(diffModel));

                                        var filePath =
                                            newType
                                                .Ref()
                                                .Id
                                                .Parts
                                                .Select(id => id.Cs())
                                                .JoinString("/")
                                                .AddRightIfNotEmpty("/");

                                        return (tree: syntaxTree, filePath: $"{filePath}{newType.Id.Value}.g");
                                    })
                                .ToList();

                        foreach (var treeWithPath in syntaxTreesWithPaths)
                        {
                            var formattedGeneratedSrc =
                                Formatter.Format(treeWithPath.tree.GetRoot(), new AdhocWorkspace()).ToFullString();

                            sourceProductionContext.AddSource(treeWithPath.filePath, formattedGeneratedSrc);
                        }
                    });
            });
    }

    #region Private

    private Dictionary<TsTypeRef, TypeGenerator> _typeGenerators = new();

    private static IEnumerable<INamedTypeSymbol> CollectDependentTypes(INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol
           .TsAttributes()
           .SelectMany(
               attributeData => attributeData
                                .AttributeClass
                                ?.TypeArguments
                                .Select(arg => arg))
           .Concat(namedTypeSymbol.ContainingTypeHierarchy())
           .Select(depType => (INamedTypeSymbol)depType);

    private static (TsModel model, Maybe<INamedTypeSymbol> firstErrorSymbol) CreateModel(
        ImmutableArray<INamedTypeSymbol> typeSymbols)
    {
        try
        {
            return (typeSymbols
                    .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
                    .Aggregate(
                        TsModel.New(),
                        (current, typeSymbol) => current.AddType(typeSymbol.ToType())),
                Maybe<INamedTypeSymbol>.NONE);
        }
        catch (TsModelCreationSymbolErrorException e)
        {
            return (new TsModel(new TsDict<TsTypeRef, TsType>()), Maybe<INamedTypeSymbol>.Some(e.Symbol));
        }
    }

    private static IEnumerable<TsType> OrderTypesTopologicallyByDependencies(TsModel model)
    {
        var depGraph = new TypeDependencyGraph();

        foreach (var type in model.Types)
        {
            depGraph.Add(type);
            foreach (var depRef in type.Attrs.SelectMany(targetAttr => targetAttr.TypeArgs))
            {
                depGraph.AddDependency(type, model.Resolve(depRef));
            }
        }

        var typesOrderedTopologicallyByDependencies = depGraph.OrderTypesTopologicallyByDependencies();
        return typesOrderedTopologicallyByDependencies;
    }

    private static bool RunDiagnostics(
        TsModel model,
        TsList<(TsType targetType, TsAttr attr)> targets,
        Maybe<INamedTypeSymbol> firstErrorSymbol,
        SourceProductionContext sourceProductionContext)
    {
        if (!firstErrorSymbol.Match(
                errorSymbol =>
                {
                    EDiagnosticsCode.ModelCreationFailedBecauseOfSymbolError.ReportError(
                        sourceProductionContext,
                        "The symbol {0} contains an error.",
                        errorSymbol);
                    return false;
                },
                () => true))
        {
            return false;
        }

        foreach (var (targetType, _) in targets)
        {
            if (!Diag.RunTypeHierarchyIsPartialDiagnostics(sourceProductionContext, model, targetType))
            {
                return false;
            }
        }

        return true;
    }

    private static IncrementalValueProvider<(
        TsModel model,
        TsList<(TsType targetType, TsAttr attr)> targets,
        Maybe<INamedTypeSymbol> firstErrorSymbol)> TsTargetTypes(IncrementalGeneratorInitializationContext context)
        => context
           .SyntaxProvider
           .CreateSyntaxProvider(
               (s, _)
                   => s is InterfaceDeclarationSyntax or ClassDeclarationSyntax or RecordDeclarationSyntax,
               (ctx, cancellationToken)
                   =>
               {
                   var namedTypeSymbol =
                       (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, cancellationToken)!;
                   return namedTypeSymbol.HasTsAttribute()
                       ? TsList.Create(CollectDependentTypes(namedTypeSymbol).Append(namedTypeSymbol))
                       : TsList.Create<INamedTypeSymbol>();
               })
           .SelectMany((x, _) => x)
           .Collect()
           .Select(
               (typeSymbols, _) =>
               {
                   var t = CreateModel(typeSymbols);
                   return (
                       t.model,
                       targets: TsList.Create(
                           OrderTypesTopologicallyByDependencies(t.model)
                               .Where(type => type.Mods.TargetType.IsSet)
                               .SelectMany(
                                   targetType => targetType
                                                 .Attrs
                                                 .Where(attr => attr.IsTsAttr)
                                                 .Select(attr => (targetType, attr)))),
                       t.firstErrorSymbol);
               });

    private void InitializeTypeGenerators(IncrementalGeneratorInitializationContext context)
        => _typeGenerators =
            new List<TypeGenerator>
                {
                    new PickGenerator(), new OmitGenerator(), new TaggedUnionGenerator(), new IntersectionGenerator(),
                }
                .ToDictionary(generator => generator.AttributeDefinition(context).TypeRef());

    private void RegisterTypeGeneratorAttributes(IncrementalGeneratorInitializationContext context)
    {
        foreach (var attrDef in _typeGenerators.Values.Select(generator => generator.AttributeDefinition(context)))
        {
            context.RegisterPostInitializationOutput(
                ctx
                    => ctx.AddSource(
                        $"{attrDef.Id.Value}.g.cs",
                        SourceText.From(attrDef.Cs(), Encoding.UTF8)));
        }
    }

    private Maybe<TsModel> RunGenerators(
        TsModel model,
        TsList<(TsType targetType, TsAttr attr)> targets,
        SourceProductionContext sourceProductionContext)
    {
        var generatedModel = model;

        foreach (var (targetType, targetAttr) in targets)
        {
            var typeGenerator = _typeGenerators[targetAttr.Type];

            if (!typeGenerator.RunDiagnostics(sourceProductionContext, generatedModel, targetType, targetAttr))
            {
                return Maybe<TsModel>.NONE;
            }

            try
            {
                generatedModel = typeGenerator.Generate(targetType, targetAttr, generatedModel);
            }
            catch (TsGeneratorException e)
            {
                e.Code.ReportError(sourceProductionContext, e.FmtMsg, e.FmtArgs);
                return Maybe<TsModel>.NONE;
            }
        }

        return generatedModel;
    }

    #endregion
}
