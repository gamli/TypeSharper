using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model.Member;

namespace TypeSharper.SemanticExtensions;

public static class ParameterSymbolExtensions
{
    public static TsParam ToParam(this IParameterSymbol parameterSymbol)
        => new(
            parameterSymbol.Type.ToTypeRef(),
            parameterSymbol.ToId(),
            parameterSymbol.IsParams);

    public static TsList<TsParam> ToParams(this IEnumerable<IParameterSymbol> parameterSymbols)
        => TsList.Create(parameterSymbols.Select(paramSymbol => paramSymbol.ToParam()));
}
