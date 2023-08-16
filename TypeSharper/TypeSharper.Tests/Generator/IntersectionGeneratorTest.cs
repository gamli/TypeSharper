using System.Linq;
using Xunit;

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
                    });
        var typeNamesToIntersect =
            EnumerableExtensions.Generate(TYPE_COUNT, i => $"Type{i}").ToList();

        GeneratorTest.ExpectOutput(
            // language=csharp
            $$"""
            using TypeSharper.Attributes;
            {{typesToIntersect.JoinLines()}}
            [TypeSharperIntersection<{{typeNamesToIntersect.JoinList()}}>()]
            public partial class IntersectionTarget { }
            """,
            typeNamesToIntersect
                .Select(
                    typeName =>
                        // language=csharp
                        $$"""
                        public IntersectionTarget({{typeName}} valueToConvert)
                        {
                            Prop10 = valueToConvert.Prop10;
                        }
                        """)
                .Append(
                    // language=csharp
                    $"public System.Int32 Prop{TYPE_COUNT} {{ get; set; }}")
                .Append(
                    // language=csharp
                    "public partial class IntersectionTarget")
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
            [TypeSharperIntersection<Type1, Type2>()]
            public partial record IntersectionTarget { }
            """,
            // language=csharp
            """
            public partial record IntersectionTarget(System.Int32 A, System.Int32 B)
            """,
            // language=csharp
            """
            public static implicit operator IntersectionTarget(Type1 valueToCast)
                => new (valueToCast.A, valueToCast.B);
            """,
            // language=csharp
            """
            public static implicit operator IntersectionTarget(Type2 valueToCast)
                => new (valueToCast.A, valueToCast.B);
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
            [TypeSharperIntersection<Type1, Type2>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            """
            public static implicit operator IntersectionTarget(Type1 valueToCast)
                => new IntersectionTarget
                {
                    A = valueToCast.A,
                    C = valueToCast.C
                };
            """,
            // language=csharp
            """
            public static implicit operator IntersectionTarget(Type2 valueToCast)
                => new IntersectionTarget
                {
                    A = valueToCast.A,
                    C = valueToCast.C
                };
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
            [TypeSharperIntersection<Type1, Type2>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type1 valueToConvert)
            {
                A = valueToConvert.A;
                C = valueToConvert.C;
            }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type2 valueToConvert)
            {
                A = valueToConvert.A;
                C = valueToConvert.C;
            }
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
            [TypeSharperIntersection<Type1, Type2>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            "public partial class IntersectionTarget",
            // language=csharp
            "public System.Int32 A { get; set; }",
            // language=csharp
            "public System.Int32 C { get; set; }");

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
            [TypeSharperIntersection<Type1, Type2>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type1 valueToConvert)
            {
                
            }
            """,
            // language=csharp
            """
            public IntersectionTarget(Type2 valueToConvert)
            {
                
            }
            """);
}
