using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class IntersectionGeneratorTests
{
    [Fact]
    public void Intersect_three_types()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public class FirstCase
            {
                public int A { get; set; }
                public int B { get; set; }
                public int C { get; set; }
            }
            public class SecondCase
            {
                public int A { get; set; }
                public int B { get; set; }
            }
            public class ThirdCase
            {
                public int A { get; set; }
            }
            [TypeSharperIntersection<FirstCase, SecondCase, ThirdCase>()]
            public partial class IntersectionTarget { }
            """,
            // language=csharp
            """
            public partial class IntersectionTarget
            {
                public System.Int32 A { get; set; }
            }
            """);
}
