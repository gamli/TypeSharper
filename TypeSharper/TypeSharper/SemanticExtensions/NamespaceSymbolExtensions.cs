using Microsoft.CodeAnalysis;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.SemanticExtensions;

public static class NamespaceSymbolExtensions
{
    public static TsNs ToNs(this INamespaceSymbol? namespaceSymbol)
        => namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace
            ? new TsNs(new TsQualifiedId())
            : new TsNs(namespaceSymbol.ToQualifiedId());

    public static TsQualifiedId ToNsRef(this INamespaceSymbol? namespaceSymbol)
        => namespaceSymbol == null || namespaceSymbol.IsGlobalNamespace
            ? new TsQualifiedId()
            : namespaceSymbol.ToQualifiedId();
}
