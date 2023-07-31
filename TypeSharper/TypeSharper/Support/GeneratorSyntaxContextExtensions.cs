using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.SyntaxExtensions;

namespace TypeSharper.Support;

public class GeneratorSyntaxContextExtensions
{
    #region Private

    private static (TypeDeclarationSyntax declaration, IMethodSymbol? attributeSymbol) TypeAndAttribute(
        GeneratorSyntaxContext context,
        string attributeName,
        string attributeNamespace)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        foreach (var attributeSyntax in typeDeclaration.Attributes())
        {
            var attributeSymbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);

            if (attributeSymbolInfo.Symbol is not IMethodSymbol attributeSymbol)
            {
                // if we can't get the symbol, ignore it
                continue;
            }

            if (attributeSymbol.ContainingType.OriginalDefinition.ToDisplayString()
                == attributeNamespace + "." + attributeName)
            {
                return (typeDeclaration, attributeSymbol);
            }
        }

        return (typeDeclaration, null);
    }

    #endregion
}
