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
    public void Pick_from_interface()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperPickAttribute<IPickSource>(nameof(PickSource.Name), nameof(PickSource.IsSample))]
            public partial interface IPickTarget { }
            """,
            // language=csharp
            """
            public partial interface IPickTarget
            {
                public System.String Name { get; set; }
                public System.Boolean IsSample { get; set; }
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
}
