using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypeSharper;

public class TypeDependencyGraph
{
    public void Add(INamedTypeSymbol typeSymbol)
    {
        if (!_graph.ContainsKey(typeSymbol))
        {
            _graph.Add(typeSymbol, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
        }
    }

    public void AddDependency(INamedTypeSymbol typeSymbol, INamedTypeSymbol dependantTypeSymbol)
    {
        if (!_graph.ContainsKey(typeSymbol))
        {
            _graph.Add(typeSymbol, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
        }

        _graph[typeSymbol].Add(dependantTypeSymbol);
    }

    public IEnumerable<INamedTypeSymbol> OrderTypesTopologicallyByDependencies()
        => OrderTypesTopologicallyByDependenciesReverse().Reverse();

    #region Private

    private readonly Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> _graph =
        new(SymbolEqualityComparer.Default);

    private IEnumerable<INamedTypeSymbol> OrderTypesTopologicallyByDependenciesReverse()
    {
        var incomingEdgesCounts =
            _graph.Keys.ToDictionary<INamedTypeSymbol, INamedTypeSymbol, int>(
                k => k,
                _ => 0,
                SymbolEqualityComparer.Default);

        foreach (var dependantType in _graph.Values.SelectMany(values => values))
        {
            if (incomingEdgesCounts.ContainsKey(dependantType))
            {
                incomingEdgesCounts[dependantType]++;
            }
        }

        while (incomingEdgesCounts.Values.Any(count => count != -1))
        {
            if (incomingEdgesCounts.All(kv => kv.Value != 0))
            {
                throw new CircularDependencyException();
            }

            var firstTypeWithoutIncomingEdges =
                incomingEdgesCounts.First(kv => kv.Value == 0).Key;

            foreach (var dependantType
                     in _graph[firstTypeWithoutIncomingEdges]
                         .Where(dependantType => incomingEdgesCounts.ContainsKey(dependantType)))
            {
                incomingEdgesCounts[dependantType]--;
            }

            incomingEdgesCounts[firstTypeWithoutIncomingEdges] = -1;

            yield return firstTypeWithoutIncomingEdges;
        }
    }

    #endregion

    #region Nested types

    public class CircularDependencyException : Exception { }

    #endregion
}
