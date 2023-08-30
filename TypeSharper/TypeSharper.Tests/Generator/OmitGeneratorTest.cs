using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class OmitGeneratorTests
{
    [Fact]
    public void A_constructor_that_takes_the_from_type_as_argument_is_generated()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record OmitSource(string Name, bool IsSample, int Count);
            [TypeSharper.Attributes.TsOmitAttribute<OmitSource>("Count")]
            public partial record OmitTarget;
            """,
            // language=csharp
            """
            public partial record OmitTarget(System.String Name, System.Boolean IsSample)
            """,
            // language=csharp
            """
            public OmitTarget(OmitSource from)
            : this(from.Name, from.IsSample) { }
            """);

    [Fact]
    public void An_implicit_cast_operator_that_takes_the_from_type_as_argument_is_generated()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record OmitSource(string Name, bool IsSample, int Count);
            [TypeSharper.Attributes.TsOmitAttribute<OmitSource>("Count")]
            public partial record OmitTarget;
            """,
            // language=csharp
            """
            public static implicit operator OmitTarget(OmitSource from)
                => new(from);
            """);

    [Fact]
    public void Omit_a_single_property()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public class OmitSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
                public int Count { get; set; }
            }
            [TypeSharper.Attributes.TsOmitAttribute<OmitSource>(nameof(OmitSource.Count))]
            public partial record OmitTarget { }
            """,
            // language=csharp
            "public partial record OmitTarget(System.String Name, System.Boolean IsSample)");

    [Fact]
    public void Omit_multiple_properties()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public class OmitSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
                public int Count { get; set; }
            }
            [TypeSharper.Attributes.TsOmitAttribute<OmitSource>(nameof(OmitSource.Count), "IsSample")]
            public partial record OmitTarget { }
            """,
            // language=csharp
            "public partial record OmitTarget(System.String Name)");

    [Fact]
    public void Omitting_non_existing_properties_is_an_error()
        => GeneratorTest.Fail(
            EDiagnosticsCode.PropertyDoesNotExist,
            // language=csharp
            """
            public class OmitSource
            {
                public string Name { get; set; }
            }
            [TypeSharper.Attributes.TsOmitAttribute<OmitSource>("Abc", "Xyz")]
            public partial record OmitTarget;
            """);
}
