using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class PickGeneratorTests
{
    [Fact]
    public void A_constructor_that_takes_the_from_type_as_argument_is_generated()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record PickSource(string Name, bool IsSample, int Count);
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>("Count")]
            public partial record PickTarget;
            """,
            // language=csharp
            """
            public partial record PickTarget(System.Int32 Count)
            """,
            // language=csharp
            """
            public PickTarget(PickSource fromValue)
            : this(fromValue.Count) { }
            """);

    [Fact]
    public void An_implicit_cast_operator_that_takes_the_from_type_as_argument_is_generated()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record PickSource(string Name, bool IsSample, int Count);
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>("Count")]
            public partial record PickTarget;
            """,
            // language=csharp
            """
            public static implicit operator PickTarget(PickSource fromValue)
                => new(fromValue);
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
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>(nameof(PickSource.Count))]
            public partial class PickTarget { }
            """,
            // language=csharp
            "public partial class PickTarget",
            // language=csharp
            "public System.Int32 Count { get; set; }");

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
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>(nameof(PickSource.Count), "IsSample")]
            public partial class PickTarget { }
            """,
            // language=csharp
            "public partial class PickTarget",
            // language=csharp
            "public System.Int32 Count { get; set; }",
            // language=csharp
            "public System.Boolean IsSample { get; set; }");

    [Fact]
    public void Picking_a_non_existing_property_is_an_error()
        => GeneratorTest.Fail(
            // language=csharp
            EDiagnosticsCode.PropertyDoesNotExist,
            """
            public class PickSource { }
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>("Name")]
            public partial class PickTarget { }
            """);
}
