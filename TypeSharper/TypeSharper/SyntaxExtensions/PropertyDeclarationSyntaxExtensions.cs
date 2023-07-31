using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeSharper.SyntaxExtensions;

public static class PropertyDeclarationSyntaxExtensions
{
    public static bool HasAccessor(this PropertyDeclarationSyntax propertyDecl, SyntaxKind accessorKind)
        => propertyDecl.AccessorList?.Accessors.Any(accessorKind) ?? false;

    public static bool HasGetAccessor(this PropertyDeclarationSyntax propertyDecl)
        => propertyDecl.HasAccessor(SyntaxKind.GetKeyword);

    public static bool HasInitAccessor(this PropertyDeclarationSyntax propertyDecl)
        => propertyDecl.HasAccessor(SyntaxKind.InitKeyword);

    public static bool HasSetAccessor(this PropertyDeclarationSyntax propertyDecl)
        => propertyDecl.HasAccessor(SyntaxKind.InitKeyword);
}
