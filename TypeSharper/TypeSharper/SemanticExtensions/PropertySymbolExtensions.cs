using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;

namespace TypeSharper.SemanticExtensions;

public static class PropertySymbolExtensions
{
    public static SyntaxNode Syntax(this ISymbol symbol) => symbol.DeclaringSyntaxReferences.Single().GetSyntax();


    // public static TsMemberMods ToMemberMods(this IPropertySymbol propertySymbol)
    //     => new(
    //         propertySymbol.DeclaredAccessibility.ToVisibility(),
    //         new TsAbstractMod(
    //             propertySymbol.IsAbstract && propertySymbol.ContainingType.TypeKind != TypeKind.Interface),
    //         new TsStaticMod(propertySymbol.IsStatic),
    //         ETsOperator.None);

    public static TsProp ToProp(this IPropertySymbol propertySymbol)
        => new(propertySymbol.Type.ToTypeRef(), propertySymbol.ToName());
}
