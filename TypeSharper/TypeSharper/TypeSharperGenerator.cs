using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using TypeSharper.Generator;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Type;
using TypeSharper.SemanticExtensions;
using TypeSharper.Support;

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
                var (model, targets) = t;

                if (!RunDiagnostics(model, targets, sourceProductionContext))
                {
                    return;
                }

                var maybeGeneratedModel = RunGenerators(model, targets, sourceProductionContext);

                maybeGeneratedModel.IfSome(
                    generatedModel =>
                    {
                        var diffModel = model.Diff(generatedModel);
                        foreach (var newType in diffModel.Types.Where(
                                     type => type.IsTopLevel() && type.TypeKind != TsType.EKind.Special))
                        {
                            AddGeneratedTypeSource(diffModel, newType, sourceProductionContext);
                        }
                    });
            });
    }

    #region Private

    private Dictionary<TsTypeRef, TypeGenerator> _typeGenerators = new();

    private static void AddGeneratedTypeSource(
        TsModel model,
        TsType type,
        SourceProductionContext sourceProductionContext)
    {
        var formattedGeneratedSrc =
            Formatter
                .Format(CSharpSyntaxTree.ParseText(type.Cs(model)).GetRoot(), new AdhocWorkspace())
                .ToFullString();

        var newTypeSourceDirectory =
            type
                .Ref()
                .Ns
                .Match(
                    qualified => qualified.Parts.Select(p => p.Value).JoinString("/"),
                    () => "");

        sourceProductionContext.AddSource(
            $"{newTypeSourceDirectory.AddRightIfNotEmpty("/")}{type.Id.Value}.g",
            formattedGeneratedSrc);
    }

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

    private static TsModel CreateModel(ImmutableArray<INamedTypeSymbol> typeSymbols)
        => typeSymbols
           .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
           .Aggregate(
               TsModel.New(),
               (current, typeSymbol) => current.AddType(typeSymbol.ToType()));

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
        SourceProductionContext sourceProductionContext)
    {
        foreach (var (targetType, _) in targets)
        {
            if (!EDiagnosticsCode.TypeHierarchyMustBePartial.RunDiagnostics(sourceProductionContext, model, targetType))
            {
                return false;
            }
        }

        return true;
    }

    private static IncrementalValueProvider<(TsModel model, TsList<(TsType targetType, TsAttr attr)> targets)>
        TsTargetTypes(IncrementalGeneratorInitializationContext context)
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
                   var model = CreateModel(typeSymbols);
                   return (
                       model,
                       targets: TsList.Create(
                           OrderTypesTopologicallyByDependencies(model)
                               .Where(type => type.Mods.TargetType.IsSet)
                               .SelectMany(
                                   targetType => targetType
                                                 .Attrs
                                                 .Where(attr => attr.IsTsAttr)
                                                 .Select(attr => (targetType, attr)))));
               });

    private void InitializeTypeGenerators(IncrementalGeneratorInitializationContext context)
        => _typeGenerators =
            new List<TypeGenerator>
                {
                    new PickGenerator(), new OmitGenerator(), new UnionGenerator(), new IntersectionGenerator(),
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

            generatedModel = typeGenerator.Generate(targetType, targetAttr, generatedModel);
        }

        return Maybe<TsModel>.Some(generatedModel);
    }

    #endregion
}
