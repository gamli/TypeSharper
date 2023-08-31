using System.Linq;
using TypeSharper.Diagnostics;
using Xunit;

namespace TypeSharper.Tests.Generator;

public class IntersectionGeneratorTests
{
    [Fact]
    public void Intersecting_tagged_union_types_is_an_error()
        => GeneratorTest.Fail(
            EDiagnosticsCode.IntersectionOfTaggedUnionsIsNotSupported,
            // ReSharper disable once HeapView.ObjectAllocation
            // language=csharp
            """
            using TypeSharper.Attributes;

            [TsTaggedUnionAttribute<int, string>("AnInt", "AString")]
            public abstract partial record FirstUnion { }

            [TsTaggedUnionAttribute<bool>("ABool", "Empty")]
            public abstract partial record SecondUnion { }
            
            [TsIntersection<FirstUnion, SecondUnion>()]
            public partial record IntersectionTarget { }
            """);
    
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
                    });
        var typeNamesToIntersect =
            EnumerableExtensions.Generate(TYPE_COUNT, i => $"Type{i}").ToList();

        GeneratorTest.ExpectOutput(
            // language=csharp
            $$"""
            using TypeSharper.Attributes;
            {{typesToIntersect.JoinLines()}}
            [TsIntersection<{{typeNamesToIntersect.JoinList()}}>()]
            public partial record IntersectionTarget;
            """,
            typeNamesToIntersect
                .Select(
                    typeName =>
                        // language=csharp
                        $$"""
                        public IntersectionTarget({{typeName}} from)
                        : this(from.Prop{{TYPE_COUNT}}) { }
                        """)
                .Append(
                    // language=csharp
                    $"public partial record IntersectionTarget(System.Int32 Prop{TYPE_COUNT})")
                .ToArray());
    }

    [Fact]
    public void Intersecting_records_will_create_a_primary_constructor()
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
                public int B { get; set; }
                public int D { get; set; }
            }
            [TsIntersection<Type1, Type2>()]
            public partial record IntersectionTarget { }
            """,
            // language=csharp
            """
            public partial record IntersectionTarget(System.Int32 A, System.Int32 B)
            """);

    [Fact]
    public void Intersecting_two_types_creates_cast_operators_for_each_type()
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
            [TsIntersection<Type1, Type2>()]
            public partial record IntersectionTarget { }
            """,
            // language=csharp
            """
            public static implicit operator IntersectionTarget(Type1 from)
                => new (from);
            """,
            // language=csharp
            """
            public static implicit operator IntersectionTarget(Type2 from)
                => new (from);
            """);

    [Fact]
    public void Intersecting_two_types_creates_constructors_for_each_type()
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
            [TsIntersection<Type1, Type2>()]
            public partial record IntersectionTarget { }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type1 from)
            : this(from.A, from.C) { }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type2 from)
            : this(from.A, from.C) { }
            """);

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
            [TsIntersection<Type1, Type2>()]
            public partial record IntersectionTarget { }
            """,
            // language=csharp
            "public partial record IntersectionTarget(System.Int32 A, System.Int32 C)");

    [Fact]
    public void Intersecting_two_types_with_no_properties_in_common_generates_empty_constructors()
        => GeneratorTest.ExpectOutput(
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
            [TsIntersection<Type1, Type2>()]
            public partial record IntersectionTarget;
            """,
            // language=csharp
            """
            public IntersectionTarget(Type1 _)
            { }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type2 _)
            { }
            """);
}
