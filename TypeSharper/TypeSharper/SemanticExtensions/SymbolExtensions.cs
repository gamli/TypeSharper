using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model;

namespace TypeSharper.SemanticExtensions;

public static class SymbolExtensions
{
    public static bool HasTsAttribute(this ISymbol symbol) => symbol.TsAttributes().Any();

    public static bool IsPrimaryCtor(this ISymbol symbol)
        => symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
            is RecordDeclarationSyntax { ParameterList: not null };

    public static bool ShouldBeIgnored(this ISymbol symbol)
        => symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected;

    public static TsName ToName(this ISymbol symbol) => new(symbol.Name);

    public static TsQualifiedName ToQualifiedId(this ISymbol symbol)
        => new(
            TsList.Create(
                symbol
                    .ToDisplayParts()
                    .Where(part => part.Kind != SymbolDisplayPartKind.Punctuation)
                    .Select(part => new TsName(part.ToString()))));

    public static IEnumerable<AttributeData> TsAttributes(this ISymbol symbol)
        => symbol.GetAttributes().Where(AttributeDataExtensions.IsTsAttribute);
}
