using TypeSharper.Support;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class IntersectionGeneratorTests
{
    [Fact]
    public void Intersecting_many_types_produces_a_type_with_properties_present_in_all()
    {
        const int TYPE_COUNT = 10;

        var typesToIntersect =
            EnumerableExtensions
                .Generate(
                    TYPE_COUNT,
                    i =>
                    {
                        var properties =
                            EnumerableExtensions
                                .Generate(TYPE_COUNT + 1, j => j == i ? "" : $"public int Prop{j} {{ get; set; }}")
                                .WhereNotNullOrWhitespace()
                                .JoinLines();
                        // language=csharp
                        return $$"""
                            public class Type{{i}}
                            {
                            {{properties.Indent()}}
                            }
                            """;
                    })
                .JoinLines();
        var typeNamesToIntersect = EnumerableExtensions.Generate(TYPE_COUNT, i => $"Type{i}").JoinList();

        GeneratorTest.ExpectOutput(
            // language=csharp
            $$"""
            using TypeSharper.Attributes;
            {{typesToIntersect}}
            [TypeSharperIntersection<{{typeNamesToIntersect}}>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            $$"""
            public partial class IntersectionTarget
            {
                public System.Int32 Prop{{TYPE_COUNT}} { get; set; }
            }
            """);
    }

    [Fact]
    public void Intersecting_two_types_produces_a_type_with_properties_present_in_both()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public class Type1
            {
                public int A { get; set; }
                public int B { get; set; }
                public int C { get; set; }
            }
            public class Type2
            {
                public int A { get; set; }
                public int C { get; set; }
            }
            [TypeSharperIntersection<Type1, Type2>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            """
            public partial class IntersectionTarget
            {
                public System.Int32 A { get; set; }
                public System.Int32 C { get; set; }
            }
            """);

    [Fact]
    public void Intersecting_two_types_with_no_properties_in_common_does_not_generate_anything()
        => GeneratorTest.ExpectEmptyOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public class Type1
            {
                public int A { get; set; }
            }
            public class Type2
            {
                public int B { get; set; }
            }
            [TypeSharperIntersection<Type1, Type2>()]
            public partial class IntersectionTarget { }
            """);
}
