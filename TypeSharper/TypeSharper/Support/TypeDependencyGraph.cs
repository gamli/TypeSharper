using System;
using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Type;

namespace TypeSharper.Support;

public class TypeDependencyGraph
{
    public void Add(TsType tsTypeParts)
    {
        if (!_graph.ContainsKey(tsTypeParts))
        {
            _graph.Add(tsTypeParts, new List<TsType>());
        }
    }

    public void AddDependency(TsType tsTypeParts, TsType dependantTsType)
    {
        if (!_graph.ContainsKey(tsTypeParts))
        {
            _graph.Add(tsTypeParts, new List<TsType>());
        }

        _graph[tsTypeParts].Add(dependantTsType);
    }

    public IEnumerable<TsType> OrderTypesTopologicallyByDependencies()
        => OrderTypesTopologicallyByDependenciesReverse().Reverse();

    #region Private

    private readonly Dictionary<TsType, List<TsType>> _graph = new();

    private IEnumerable<TsType> OrderTypesTopologicallyByDependenciesReverse()
    {
        var incomingEdgesCounts = _graph.Keys.ToDictionary(k => k, _ => 0);

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

            foreach (var dependantType in _graph[firstTypeWithoutIncomingEdges])
            {
                if (incomingEdgesCounts.ContainsKey(dependantType))
                {
                    incomingEdgesCounts[dependantType]--;
                }
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
