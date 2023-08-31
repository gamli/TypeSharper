using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TypeSharper.Diagnostics;
using Xunit;

namespace TypeSharper.Tests.Generator;

[SuppressMessage("ReSharper", "HeapView.ObjectAllocation")]
public class ProductGeneratorTests
{
    private const string _INPUT_TYPE1_TYPE2 =
        // language=csharp
        """
        using TypeSharper.Attributes;
        public class Type1
        {
            public int A { get; set; }
            public int B { get; set; }
        }
        public class Type2
        {
            public int C { get; set; }
            public int D { get; set; }
        }
        [TsProduct<Type1, Type2>()]
        public partial record ProductTarget { }
        """;

    [Fact]
    public void Product_of_two_records_creates_a_record_with_the_properties_of_both_in_a_primary_constructor()
        => GeneratorTest.ExpectOutput(
            _INPUT_TYPE1_TYPE2,
            // language=csharp
            """
            public partial record ProductTarget(
                System.Int32 A,
                System.Int32 B,
                System.Int32 C,
                System.Int32 D)
            """);
    
    [Fact]
    public void Ctor_param_names_are_suffixed_with_the_type_name()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            namespace Some.Namespace.That.Contains.Dots;
            public class Type
            {
                public int A { get; set; }
                public int B { get; set; }
            }
            [TsProduct<Type>()]
            public partial record ProductTarget { }
            """,
            // language=csharp
            """
            public ProductTarget(Some.Namespace.That.Contains.Dots.Type fromSome_Namespace_That_Contains_Dots_Type)
                : this(fromSome_Namespace_That_Contains_Dots_Type.A, fromSome_Namespace_That_Contains_Dots_Type.B) { }
            """,
            // language=csharp
            "namespace Some.Namespace.That.Contains.Dots;");

    [Fact]
    public void Product_of_two_records_creates_a_constructor_that_takes_both_types_as_parameters()
        => GeneratorTest.ExpectOutput(
            _INPUT_TYPE1_TYPE2,
            // language=csharp
            "public ProductTarget(Type1 fromType1, Type2 fromType2)",
            // language=csharp
            ": this(fromType1.A, fromType1.B, fromType2.C, fromType2.D)");

    [Fact]
    public void Product_of_tagged_union_types_is_an_error()
        => GeneratorTest.Fail(
            EDiagnosticsCode.ProductOfTaggedUnionsIsNotSupported,
            // ReSharper disable once HeapView.ObjectAllocation
            // language=csharp
            """
            using TypeSharper.Attributes;

            [TsTaggedUnionAttribute<int, string>("AnInt", "AString")]
            public abstract partial record FirstUnion { }

            [TsTaggedUnionAttribute<bool>("ABool", "Empty")]
            public abstract partial record SecondUnion { }

            [TsProduct<FirstUnion, SecondUnion>()]
            public partial record ProductTarget { }
            """);

    [Fact]
    public void Product_of_many_types_produces_a_type_with_all_the_properties_combined()
    {
        const int TYPE_COUNT = 10;
        const int PROP_COUNT = 5;

        var typesToMultiply =
            EnumerableExtensions
                .Generate(
                    TYPE_COUNT,
                    i =>
                    {
                        var properties =
                            EnumerableExtensions
                                .Generate(
                                    PROP_COUNT,
                                    j => $"public int Prop{i}_{j} {{ get; set; }}")
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

        var typeNamesToMultiply =
            EnumerableExtensions.Generate(TYPE_COUNT, i => $"Type{i}").ToList();

        var propertyAccesses =
            EnumerableExtensions
                .Generate(
                    TYPE_COUNT,
                    i => EnumerableExtensions
                        .Generate(PROP_COUNT, j => $"fromType{i}.Prop{i}_{j}"))
                .Flatten();

        var primaryCtorParameters =
            EnumerableExtensions
                .Generate(
                    TYPE_COUNT,
                    i => EnumerableExtensions
                        .Generate(PROP_COUNT, j => $"System.Int32 Prop{i}_{j}"))
                .Flatten();
        
        var constituentTypeCtorParameters =
            typeNamesToMultiply.Select(typeName => $"{typeName} from{typeName}");

        GeneratorTest.ExpectOutput(
            // language=csharp
            $$"""
            using TypeSharper.Attributes;
            {{typesToMultiply.JoinLines()}}
            [TsProduct<{{typeNamesToMultiply.JoinList()}}>()]
            public partial record ProductTarget;
            """,
            // language=csharp
            $"public partial record ProductTarget({primaryCtorParameters.JoinList()})",
            // language=csharp
            $$"""
            public ProductTarget({{constituentTypeCtorParameters.JoinList()}})
            : this ({{propertyAccesses.JoinList()}}) { }
            """);
    }
}
