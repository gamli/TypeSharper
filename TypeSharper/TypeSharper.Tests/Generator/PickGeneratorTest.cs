using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class PickGeneratorTests
{
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
            """
            public partial class PickTarget
            {
                public System.Int32 Count { get; set; }
            }
            """);

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
            """
            public partial class PickTarget
            {
                public System.Int32 Count { get; set; }
                public System.Boolean IsSample { get; set; }
            }
            """);
    
    [Fact]
    public void Picking_no_property_generates_no_code()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public class PickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
                public int Count { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>()]
            public partial class PickTarget { }
            """);

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
