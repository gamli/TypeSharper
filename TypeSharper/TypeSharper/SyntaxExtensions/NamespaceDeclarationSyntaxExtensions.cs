using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeSharper.SyntaxExtensions;

public static class NamespaceDeclarationSyntaxExtensions
{
    public static string NamespaceName(this NamespaceDeclarationSyntax namespaceDeclaration)
        => namespaceDeclaration.Name.ToString();
}
