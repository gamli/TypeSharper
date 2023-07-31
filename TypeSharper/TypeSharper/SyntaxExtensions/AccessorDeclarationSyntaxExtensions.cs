using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;

namespace TypeSharper.SyntaxExtensions;

public static class AccessorDeclarationSyntaxExtensions
{
    public static TsPropAccessor ToPropAccessor(this AccessorDeclarationSyntax accessor)
        => new(accessor.ToPropAccessorKind(), accessor.Visibility());
    // public static bool IsInternal(this AccessorDeclarationSyntax accessor) => accessor.Modifiers.ContainsInternal();
    // public static bool IsPrivate(this AccessorDeclarationSyntax accessor) => accessor.Modifiers.ContainsPrivate();
    // public static bool IsProtected(this AccessorDeclarationSyntax accessor) => accessor.Modifiers.ContainsProtected();
    // public static bool IsPublic(this AccessorDeclarationSyntax accessor) => accessor.Modifiers.ContainsPublic();

    public static TsPropAccessor.EKind ToPropAccessorKind(this AccessorDeclarationSyntax accessor)
        => accessor.Kind() switch
        {
            SyntaxKind.GetAccessorDeclaration  => TsPropAccessor.EKind.Get,
            SyntaxKind.InitAccessorDeclaration => TsPropAccessor.EKind.Init,
            SyntaxKind.SetAccessorDeclaration  => TsPropAccessor.EKind.Set,
            _                                  => throw new ArgumentOutOfRangeException(),
        };

    public static ETsVisibility Visibility(this AccessorDeclarationSyntax accessor) => accessor.Modifiers.Visibility();
}
