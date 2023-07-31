using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model.Identifier;

namespace TypeSharper.SemanticExtensions;

public static class SymbolExtensions
{
    public static bool HasTsAttribute(this ISymbol symbol) => symbol.TsAttributes().Any();

    public static TsId ToId(this ISymbol symbol) => new(symbol.Name);

    public static TsQualifiedId ToQualifiedId(this ISymbol symbol)
        => new(
            TsList.Create(
                symbol
                    .ToDisplayParts()
                    .Where(part => part.Kind != SymbolDisplayPartKind.Punctuation)
                    .Select(part => new TsId(part.ToString()))));

    public static IEnumerable<AttributeData> TsAttributes(this ISymbol symbol)
        => symbol.GetAttributes().Where(AttributeDataExtensions.IsTsAttribute);
}
