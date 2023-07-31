using Microsoft.CodeAnalysis;
using TypeSharper.Model.Type;

namespace TypeSharper.SemanticExtensions;

public static class NamespaceSymbolExtensions
{
    public static TsNs ToNs(this INamespaceSymbol? namespaceSymbol)
        => namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace
            ? TsNs.Global
            : TsNs.Qualified(namespaceSymbol.ToQualifiedId());
}
