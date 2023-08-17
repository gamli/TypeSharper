using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeSharper.SyntaxExtensions;

public static class TypeDeclarationSyntaxExtensions
{
    public static IEnumerable<AttributeSyntax> Attributes(this TypeDeclarationSyntax typeDecl)
        => typeDecl
           .AttributeLists
           .SelectMany(attributeListSyntax => attributeListSyntax.Attributes);

    public static Maybe<NamespaceDeclarationSyntax> ContainingNamespace(this TypeDeclarationSyntax typeDecl)
    {
        var parent = typeDecl.Parent;
        while (parent != null)
        {
            if (parent is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                return namespaceDeclaration;
            }

            parent = parent.Parent;
        }

        return Maybe<NamespaceDeclarationSyntax>.NONE;
    }

    public static Maybe<TypeDeclarationSyntax> ContainingType(this TypeDeclarationSyntax typeDecl)
    {
        var parent = typeDecl.Parent;
        while (parent != null)
        {
            if (parent is TypeDeclarationSyntax containingType)
            {
                return containingType;
            }

            parent = parent.Parent;
        }

        return Maybe<TypeDeclarationSyntax>.NONE;
    }

    public static bool IsAbstract(this TypeDeclarationSyntax typeDecl)
        => typeDecl.Modifiers.Any(SyntaxKind.AbstractKeyword);

    public static bool IsPartial(this TypeDeclarationSyntax typeDecl)
        => typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

    public static bool IsRecord(this TypeDeclarationSyntax classDeclaration)
        => classDeclaration.Modifiers.Any(mod => mod.IsKind(SyntaxKind.RecordKeyword));

    public static bool IsSealed(this TypeDeclarationSyntax typeDecl) => typeDecl.Modifiers.ContainsSealed();

    public static bool IsStatic(this TypeDeclarationSyntax typeDecl) => typeDecl.Modifiers.ContainsStatic();

    public static string Name(this TypeDeclarationSyntax typeDecl)
        => typeDecl switch
        {
            ClassDeclarationSyntax c => c.Identifier.Text,
            StructDeclarationSyntax s => s.Identifier.Text,
            InterfaceDeclarationSyntax i => i.Identifier.Text,
            _ => throw new NotSupportedException($"Not supported syntax node of type {typeDecl.GetType().Name}"),
        };
}
