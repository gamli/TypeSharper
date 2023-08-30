using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class PickGeneratorTests
{
    [Fact]
    public void Picking_from_another_pick_type_includes_it_constructors_recursively()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public interface IPickSource
            {
                public string Name { get; set; }
                public string Description { get; set; }
            }
            [TsPickAttribute<IPickSource>("Name", "Description")]
            public partial record FirstPickTarget;
            [TsOmitAttribute<FirstPickTarget>("Description")]
            public partial record SecondOmitTarget;
            [TsPickAttribute<SecondOmitTarget>("Name")]
            public partial record ThirdPickTarget;
            """,
            ("FirstPickTarget.g.cs", new[] { "" }),
            ("SecondOmitTarget.g.cs",
                new[]
                {
                    // language=csharp
                    "public partial record SecondOmitTarget(System.String Name)",
                    // language=csharp
                    "public SecondOmitTarget(IPickSource from) : this(from.Name) { }",
                    // language=csharp
                    "public SecondOmitTarget(FirstPickTarget from) : this(from.Name) { }",
                }),
            ("ThirdPickTarget.g.cs",
                new[]
                {
                    // language=csharp
                    "public partial record ThirdPickTarget(System.String Name)",
                    // language=csharp
                    "public ThirdPickTarget(IPickSource from) : this(from.Name) { }",
                    // language=csharp
                    "public ThirdPickTarget(FirstPickTarget from) : this(from.Name) { }",
                    // language=csharp
                    "public ThirdPickTarget(SecondOmitTarget from) : this(from.Name) { }",
                }));

    [Fact]
    public void A_constructor_that_takes_the_from_type_as_argument_is_generated()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record PickSource(string Name, bool IsSample, int Count);
            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Count")]
            public partial record PickTarget;
            """,
            // language=csharp
            """
            public partial record PickTarget(System.Int32 Count)
            """,
            // language=csharp
            """
            public PickTarget(PickSource from)
            : this(from.Count) { }
            """);

    [Fact]
    public void An_implicit_cast_operator_that_takes_the_from_type_as_argument_is_generated()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record PickSource(string Name, bool IsSample, int Count);
            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Count")]
            public partial record PickTarget;
            """,
            // language=csharp
            """
            public static implicit operator PickTarget(PickSource from)
                => new(from);
            """);

    [Fact]
    public void Pick_a_single_property()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public class PickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
                public int Count { get; set; }
            }
            [TypeSharper.Attributes.TsPickAttribute<PickSource>(nameof(PickSource.Count))]
            public partial record PickTarget;
            """,
            // language=csharp
            "public partial record PickTarget(System.Int32 Count)");

    [Fact]
    public void Pick_multiple_properties()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public class PickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
                public int Count { get; set; }
            }
            [TypeSharper.Attributes.TsPickAttribute<PickSource>(nameof(PickSource.Count), "IsSample")]
            public partial record PickTarget { }
            """,
            // language=csharp
            "public partial record PickTarget(System.Boolean IsSample, System.Int32 Count)");

    [Fact]
    public void Picking_a_non_existing_property_is_an_error()
        => GeneratorTest.Fail(
            // language=csharp
            EDiagnosticsCode.PropertyDoesNotExist,
            """
            public class PickSource { }
            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Name")]
            public partial record PickTarget { }
            """);
}
