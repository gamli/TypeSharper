using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Type;
using TypeSharper.SemanticExtensions;
using TypeSharper.Support;
using Xunit;

namespace TypeSharper.Tests.Support;

public class TypeDependencyGraphTest
{
    [Fact]
    public void A_type_can_depend_on_multiple_types()
    {
        EnumerateTopologicallySorted(
                ("A", "B"),
                ("A", "C"),
                ("A", "D"),
                ("B", null),
                ("C", "E"),
                ("D", "E"),
                ("E", null))
            .Should()
            .Equal("E", "D", "C", "B", "A");
    }

    [Fact]
    public void Direct_circular_dependencies_lead_to_an_exception()
    {
        var createGraph = () => EnumerateTopologicallySorted(("A", "B"), ("B", "A")).ToList();
        createGraph.Should().Throw<TypeDependencyGraph.CircularDependencyException>();
    }

    [Fact]
    public void Indirect_circular_dependencies_lead_to_an_exception()
    {
        var createGraph = () => EnumerateTopologicallySorted(("A", "B"), ("B", "C"), ("C", "A")).ToList();
        createGraph.Should().Throw<TypeDependencyGraph.CircularDependencyException>();
    }

    [Fact]
    public void Multiple_types_can_depend_on_another_type()
    {
        EnumerateTopologicallySorted(
                ("C", "E"),
                ("A", "B"),
                ("B", "C"),
                ("D", "C"),
                ("E", null))
            .Should()
            .Equal("E", "C", "D", "B", "A");
    }

    [Fact]
    public void The_order_types_are_added_does_not_matter()
    {
        EnumerateTopologicallySorted(
                ("C", null),
                ("B", "C"),
                ("A", "B"))
            .Should()
            .Equal("C", "B", "A");
    }

    [Fact]
    public void Types_are_topologically_sorted_from_maximum_number_of_dependencies_to_lowest()
    {
        EnumerateTopologicallySorted(
                ("A", "B"),
                ("B", "C"),
                ("C", null))
            .Should()
            .Equal("C", "B", "A");
    }

    [Fact]
    public void Types_without_dependencies_are_allowed()
    {
        EnumerateTopologicallySorted(("IndependentType", null))
            .Should()
            .Equal("IndependentType");
    }

    #region Private

    private static Dictionary<string, TsType> CreateTypes(params string[] typeNames)
    {
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            typeNames.Select(typeName => CSharpSyntaxTree.ParseText($"interface {typeName} {{}}")));

        return
            compilation
                .SyntaxTrees
                .SelectMany(
                    tree =>
                    {
                        var semanticModel = compilation.GetSemanticModel(tree);
                        return tree
                               .GetRoot()
                               .DescendantNodes()
                               .OfType<TypeDeclarationSyntax>()
                               .Select(typeDecl => semanticModel.GetDeclaredSymbol(typeDecl))
                               .Where(s => s != null)
                               .Cast<INamedTypeSymbol>()
                               .Select(namedSymbol => namedSymbol.ToType());
                    })
                .ToDictionary(type => type.Id.Value);
    }

    private static IEnumerable<string> EnumerateTopologicallySorted(
        params (string type, string? dependency)[] typesWithDependencies)
    {
        var graph = new TypeDependencyGraph();

        var types =
            CreateTypes(typesWithDependencies.Select(td => td.type).Distinct().ToArray());

        foreach (var td in typesWithDependencies)
        {
            if (td.dependency == null)
            {
                graph.Add(types[td.type]);
            }
            else
            {
                graph.AddDependency(types[td.type], types[td.dependency]);
            }
        }

        return graph.OrderTypesTopologicallyByDependencies().Select(type => type.Id.Cs());
    }

    #endregion
}
