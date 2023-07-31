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
    public void Omit_from_interface()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public interface IOmitSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperOmitAttribute<IOmitSource>(nameof(OmitSource.IsSample))]
            public partial interface IOmitTarget { }
            """,
            // language=csharp
            """
            public partial interface IOmitTarget
            {
                public System.String Name { get; set; }
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
}
