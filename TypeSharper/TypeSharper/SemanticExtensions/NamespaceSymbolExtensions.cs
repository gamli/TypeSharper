using Microsoft.CodeAnalysis;
using TypeSharper.Model;

namespace TypeSharper.SemanticExtensions;

public static class NamespaceSymbolExtensions
{
    public static TsNs ToNs(this INamespaceSymbol? namespaceSymbol)
        => namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace
            ? new TsNs(new TsQualifiedName())
            : new TsNs(namespaceSymbol.ToQualifiedId());

    public static TsQualifiedName ToNsRef(this INamespaceSymbol? namespaceSymbol)
        => namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace
            ? new TsQualifiedName()
            : namespaceSymbol.ToQualifiedId();
}
