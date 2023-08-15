using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class OmitGeneratorTests
{
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
            [TypeSharper.Attributes.TypeSharperOmitAttribute<OmitSource>(nameof(OmitSource.Count))]
            public partial class OmitTarget { }
            """,
            // language=csharp
            """
            public partial class OmitTarget
            {
                public System.String Name { get; set; }
                public System.Boolean IsSample { get; set; }
            }
            """);

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
            [TypeSharper.Attributes.TypeSharperOmitAttribute<OmitSource>(nameof(OmitSource.Count), "IsSample")]
            public partial class OmitTarget { }
            """,
            // language=csharp
            """
            public partial class OmitTarget
            {
                public System.String Name { get; set; }
            }
            """);

    [Fact]
    public void Omitting_all_properties_generates_no_code()
        => GeneratorTest.ExpectEmptyOutput(
            // language=csharp
            """
            public class OmitSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
                public int Count { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperOmitAttribute<OmitSource>("Name", "IsSample", "Count")]
            public partial class PickTarget { }
            """);

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
            [TypeSharper.Attributes.TypeSharperOmitAttribute<OmitSource>("Abc", "Xyz")]
            public partial class OmitTarget { }
            """);
}
