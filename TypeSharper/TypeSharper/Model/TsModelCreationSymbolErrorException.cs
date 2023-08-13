using System;
using Microsoft.CodeAnalysis;

namespace TypeSharper.Model;

public class TsModelCreationSymbolErrorException : Exception
{
    public TsModelCreationSymbolErrorException(INamedTypeSymbol symbol)
        : base("Encountered a symbol error when creating model type: " + symbol)
        => Symbol = symbol;

    public INamedTypeSymbol Symbol { get; }
}
