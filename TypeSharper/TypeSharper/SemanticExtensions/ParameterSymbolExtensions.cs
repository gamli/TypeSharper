using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.AttrDef;

namespace TypeSharper.SemanticExtensions;

public static class ParameterSymbolExtensions
{
    public static TsAttrOverloadDef.Param ToParam(this IParameterSymbol parameterSymbol)
        => new(
            parameterSymbol.Type.ToTypeRef(),
            parameterSymbol.ToName(),
            parameterSymbol.IsParams);

    public static TsList<TsAttrOverloadDef.Param> ToParams(this IEnumerable<IParameterSymbol> parameterSymbols)
        => TsList.Create(parameterSymbols.Select(paramSymbol => paramSymbol.ToParam()));
}
