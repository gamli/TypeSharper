using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeSharper.Support;

public static class SymbolExtensions
{
    public static string CreateAdditionalTypePart(this ITypeSymbol partialType, string membersSrc)
    {
        var typeSyntax = (TypeDeclarationSyntax)partialType.DeclaringSyntaxReferences.First().GetSyntax();
        var modifiers = typeSyntax.Modifiers.ToString();
        var keyword = typeSyntax switch
        {
            InterfaceDeclarationSyntax => "interface",
            ClassDeclarationSyntax     => "class",
            RecordDeclarationSyntax    => "record",
            _                          => throw new ArgumentOutOfRangeException(),
        };

        return
            ((INamespaceOrTypeSymbol?)
             partialType.ContainingSymbol
             ?? partialType.ContainingNamespace)
            .WrapSrc(
                $$"""
                {{modifiers}} {{keyword}} {{partialType.Name}}
                {
                {{membersSrc.Indent()}}
                }
                """);
    }

    public static string WrapSrc(
        this INamespaceOrTypeSymbol? immediateParentContainer,
        string toWrapSrc)
    {
        if (string.IsNullOrEmpty(immediateParentContainer?.Name))
        {
            return toWrapSrc;
        }

        var wrappedInParent = immediateParentContainer switch
        {
            INamedTypeSymbol namedTypeSymbol => CreateAdditionalTypePart(namedTypeSymbol, toWrapSrc),
            INamespaceSymbol namespaceSymbol =>
                $$"""
                namespace {{namespaceSymbol.Name}}
                {
                {{toWrapSrc.Indent()}}
                }
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(immediateParentContainer)),
        };

        return ((INamespaceOrTypeSymbol?)immediateParentContainer.ContainingNamespace
                ?? immediateParentContainer?.ContainingType).WrapSrc(wrappedInParent);
    }
}
