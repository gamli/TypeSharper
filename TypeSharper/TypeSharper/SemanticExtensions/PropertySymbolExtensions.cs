using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model;
using TypeSharper.Model.Mod;

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
