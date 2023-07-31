using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;
using TypeSharper.SyntaxExtensions;

namespace TypeSharper.SemanticExtensions;

public static class PropertySymbolExtensions
{
    public static IEnumerable<AccessorDeclarationSyntax>? Accessors(this IPropertySymbol propertySymbol)
        => ((PropertyDeclarationSyntax)propertySymbol.Syntax()).AccessorList?.Accessors;


    public static string? CsExpressionBody(this IPropertySymbol propertySymbol)
        => ((PropertyDeclarationSyntax)propertySymbol.Syntax()).ExpressionBody?.GetText().ToString();

    public static SyntaxNode Syntax(this ISymbol symbol) => symbol.DeclaringSyntaxReferences.Single().GetSyntax();


    public static TsMemberMods ToMemberMods(this IPropertySymbol propertySymbol)
        => new(
            propertySymbol.DeclaredAccessibility.ToVisibility(),
            new TsAbstractMod(
                propertySymbol.IsAbstract && propertySymbol.ContainingType.TypeKind != TypeKind.Interface),
            new TsStaticMod(propertySymbol.IsStatic));
    // public static Maybe<string> CsBody(this IMethodSymbol methodSymbol)
    // {
    //     var csBody =
    //         ((MethodDeclarationSyntax?)methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax())
    //         ?.Body
    //         ?.GetText()
    //         .ToString();
    //     return csBody == null ? Maybe<string>.NONE : Maybe<string>.Some(csBody);
    // }


    public static TsProp ToProp(this IPropertySymbol propertySymbol)
        => new(
            propertySymbol.Type.ToTypeRef(),
            propertySymbol.ToId(),
            propertySymbol.ToMemberMods(),
            propertySymbol.ToPropBodyImpl());

    public static TsProp.BodyImpl ToPropBodyImpl(this IPropertySymbol propertySymbol)
    {
        var csExpressionBody = propertySymbol.CsExpressionBody();
        if (csExpressionBody != null)
        {
            return TsProp.BodyImpl.Expression(csExpressionBody);
        }

        return TsProp.BodyImpl.Accessors(
            TsList.Create(
                propertySymbol
                    .Accessors()
                    !.Select(accessorDecl => accessorDecl.ToPropAccessor())));
    }
}
