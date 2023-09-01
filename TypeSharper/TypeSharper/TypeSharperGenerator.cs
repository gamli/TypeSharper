using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using TypeSharper.Diagnostics;
using TypeSharper.Model;
using TypeSharper.SemanticExtensions;

namespace TypeSharper;

[Generator]
public class TypeSharperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RegisterTypeGeneratorAttributes(context);

        RegisterTypeGeneratorSupportTypes(context);

        context.RegisterSourceOutput(
            TsModelAndTargetTypes(context),
            (sourceProductionContext, modelCreationResult) =>
            {
                modelCreationResult
                    .Map(
                        modelAndTargetTypes
                            => GenerateTypes(modelAndTargetTypes, sourceProductionContext)
                                .IfSome(
                                    model =>
                                    {
                                        foreach (var generatedType in model.GeneratedTypeDict.Values)
                                        {
                                            AddSourceForType(generatedType, model, sourceProductionContext);
                                        }
                                    }),
                        error => error.Report(sourceProductionContext));
            });
    }

    #region Private

    private static void AddSourceForType(
        TsType generatedType,
        TsModel model,
        SourceProductionContext sourceProductionContext)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedType.CsFile(model));

        var filePath =
            generatedType
                .Ref()
                .Name
                .Parts
                .Select(id => id.Cs())
                .JoinPath();

        sourceProductionContext.AddSource(
            $"{filePath}{generatedType.Info.Name.Value}.g",
            CreateSource(syntaxTree));
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

    private static EitherOr<ModelAndTargetTypes, DiagnosticsError> CreateModel(
        IEnumerable<INamedTypeSymbol> namedTypeSymbols)
        => namedTypeSymbols
           .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
           .Aggregate(
               EitherOr<ModelAndTargetTypes, DiagnosticsError>
                   .Either(new ModelAndTargetTypes(TsModel.New(), TsList<TargetType>.Empty)),
               (eitherOr, typeSymbol) => eitherOr.IfEither(
                   modelAndTargetTypes =>
                   {
                       var (model, targetTypes) = modelAndTargetTypes;

                       var tsAttrs =
                           TsList.Create(
                               typeSymbol
                                   .TsAttributes()
                                   .Select(AttributeDataExtensions.ToAttr)
                                   .Where(maybeAttr => maybeAttr.Map(_ => true, () => false))
                                   .Select(maybeAttr => maybeAttr.AssertSome()));

                       if (tsAttrs.Count > 1)
                       {
                           return new DiagnosticsError(
                               EDiagnosticsCode.MultipleTsAttributesAreNotAllowed,
                               typeSymbol,
                               "The symbol {0} has more than one TypeSharper attribute. Only one attribute is allowed"
                               + " per type. This also means combinations of different attributes are not allowed:\n{1}",
                               typeSymbol,
                               tsAttrs.Select(attr => $"* {attr.GetType().Name}").JoinLines());
                       }

                       var tsAttr = tsAttrs.SingleOrDefault();
                       var tsType = typeSymbol.ToType();

                       return tsAttr is null
                           ? new ModelAndTargetTypes(model.AddType(tsType), targetTypes)
                           : new ModelAndTargetTypes(
                               model.AddType(tsType),
                               targetTypes.Add(new TargetType(tsType, tsAttrs.Single(), typeSymbol)));
                   }));

    private static string CreateSource(SyntaxTree tree)
        => $$"""
            //-----------------------------------------------------------------------//
            // <auto-generated>                                                      //
            // This code was generated by TypeSharper.                               //
            //                                                                       //
            // Changes to this file may cause incorrect behavior and will be lost if //
            // the code is regenerated.                                              //
            // </auto-generated>                                                     //
            //-----------------------------------------------------------------------//

            {{SimplifyUsings(tree.GetCompilationUnitRoot())}}
            """;

    private static Maybe<TsModel> GenerateTypes(
        ModelAndTargetTypes modelAndTargetTypes,
        SourceProductionContext sourceProductionContext)
        => modelAndTargetTypes
           .TargetTypes
           .Aggregate<Maybe<TsModel>>(
               modelAndTargetTypes.Model,
               (maybeModel, targetType) =>
                   maybeModel.IfSome(
                       model => targetType
                                .Attr
                                .RunDiagnostics(targetType.TypeSymbol, model)
                                .Map(
                                    error =>
                                    {
                                        error.Report(sourceProductionContext);
                                        return Maybe<TsModel>.NONE;
                                    },
                                    () => model.AddGeneratedType(
                                        TsTypeFactory.Create(
                                            targetType.Type.Info,
                                            targetType.Attr,
                                            model)))));

    private static IEnumerable<INamedTypeSymbol> OrderTypesTopologicallyByDependencies(
        IEnumerable<INamedTypeSymbol> types)
    {
        var depGraph = new TypeDependencyGraph();

        foreach (var type in types)
        {
            depGraph.Add(type);
            foreach (var dependentTypeSymbol
                     in type
                        .TypeSharperAttribute()
                        .SelectMany(attributeData => attributeData.AttributeClass?.TypeArguments.ToList())
                        .OfType<INamedTypeSymbol>())
            {
                depGraph.AddDependency(type, dependentTypeSymbol);
            }
        }

        var typesOrderedTopologicallyByDependencies =
            depGraph.OrderTypesTopologicallyByDependencies();

        return typesOrderedTopologicallyByDependencies;
    }

    private static void RegisterTypeGeneratorAttributes(IncrementalGeneratorInitializationContext context)
    {
        foreach (var attrDef in TypeSharperAttributes.Attributes())
        {
            context.RegisterPostInitializationOutput(
                ctx
                    => ctx.AddSource(
                        $"{attrDef.Name.Value}.g.cs",
                        SourceText.From(attrDef.Cs(), Encoding.UTF8)));
        }
    }

    private static string SimplifyUsings(CompilationUnitSyntax root)
    {
        var simplifiedRoot = root;
        // while (true)
        // {
        //     var nameSyntax = simplifiedRoot
        //                      .DescendantNodes()
        //                      .OfType<QualifiedNameSyntax>()
        //                      .FirstOrDefault(
        //                          syntax => syntax.Parent
        //                              is not BaseNamespaceDeclarationSyntax
        //                              and not UsingStatementSyntax
        //                              and not UsingDirectiveSyntax);
        //
        //     if (nameSyntax == null)
        //     {
        //         break;
        //     }
        //
        //     var simpleName = nameSyntax.Right;
        //     var namespaceName = nameSyntax.Left.ToString();
        //
        //     simplifiedRoot = simplifiedRoot.ReplaceNode(nameSyntax, simpleName);
        //
        //     if (simplifiedRoot.Usings.All(u => u.Name?.ToString() != namespaceName))
        //     {
        //         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName));
        //         simplifiedRoot = simplifiedRoot.AddUsings(usingDirective);
        //     }
        // }

        return Formatter.Format(simplifiedRoot.NormalizeWhitespace(), new AdhocWorkspace()).ToFullString();
    }

    private static IncrementalValueProvider<EitherOr<ModelAndTargetTypes, DiagnosticsError>> TsModelAndTargetTypes(
        IncrementalGeneratorInitializationContext context)
        => context
           .SyntaxProvider
           .CreateSyntaxProvider(
               (s, _)
                   => s is InterfaceDeclarationSyntax
                       or ClassDeclarationSyntax
                       or StructDeclarationSyntax
                       or RecordDeclarationSyntax,
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
               (typeSymbols, _)
                   => CreateModel(OrderTypesTopologicallyByDependencies(typeSymbols)));

    private void RegisterTypeGeneratorSupportTypes(IncrementalGeneratorInitializationContext context)
    {
        var assembly = GetType().Assembly;
        var supportTypeSourceCodes = new[]
        {
            (src: assembly.ReadEmbeddedResource("Support.Maybe.cs"),
                file: "Support/Maybe.g.cs"),
            (src: assembly.ReadEmbeddedResource("Support.Void.cs"),
                file: "Support/Void.g.cs"),
        };
        foreach (var (src, file) in supportTypeSourceCodes)
        {
            context.RegisterPostInitializationOutput(
                ctx =>
                    ctx.AddSource(file, SourceText.From(src, Encoding.UTF8)));
        }
    }

    #endregion

    #region Nested types

    private record ModelAndTargetTypes(TsModel Model, TsList<TargetType> TargetTypes);

    private record TargetType(TsType Type, TsAttr Attr, ITypeSymbol TypeSymbol);

    #endregion
}
