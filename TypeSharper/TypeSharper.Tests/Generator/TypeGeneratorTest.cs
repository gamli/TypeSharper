using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class TypeGeneratorTests
{
    [Fact]
    public void All_types_in_a_nested_type_hierarchy_must_be_partial()
        => GeneratorTest.Fail(
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            public partial interface IPickTarget
            {
                public interface INestedPickTarget
                {
                    [TypeSharper.Attributes.TypeSharperPickAttribute<IPickSource>(nameof(PickSource.Name))]
                    public partial interface INestedNestedPickTarget {}
                }
            }
            """);

    [Fact]
    public void Targeting_a_nested_type_is_possible()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            public partial interface IPickTarget
            {
                [TypeSharper.Attributes.TypeSharperPickAttribute<IPickSource>(nameof(PickSource.Name))]
                public partial interface INestedPickTarget {}
            }
            """,
            // language=csharp
            """
            public partial interface IPickTarget
            {
                public partial interface INestedPickTarget
                {
                    public System.String Name { get; set; }
                }
            }
            """);

    [Fact]
    public void Targeting_a_non_partial_type_generates_an_error()
        => GeneratorTest.Fail(
            // language=csharp
            """
            public class PickSource { }
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>(nameof(PickSource.Count))]
            public class PickTarget { }
            """);

    [Fact]
    public void Targeting_a_type_in_a_namespace_is_possible()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            namespace MyNs;
            public interface IPickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperPickAttribute<IPickSource>(nameof(PickSource.Name))]
            public partial interface IPickTarget {}
            """,
            // language=csharp
            """
            namespace MyNs;

            public partial interface IPickTarget
            {
                public System.String Name { get; set; }
            }
            """);

    [Fact]
    public void Using_a_generated_type_in_another_generated_type_is_possible()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperPickAttribute<IPickSource>(nameof(IPickSource.Name))]
            public partial interface IFirstPickTarget {}
            [TypeSharper.Attributes.TypeSharperPickAttribute<IFirstPickTarget>(nameof(IFirstPickTarget.Name))]
            public partial interface ISecondPickTarget {}
            """,
            ("IFirstPickTarget.g.cs",
                // language=csharp
                """
                public partial interface IFirstPickTarget
                {
                    public System.String Name { get; set; }
                }
                """),
            ("ISecondPickTarget.g.cs",
                // language=csharp
                """
                public partial interface ISecondPickTarget
                {
                    public System.String Name { get; set; }
                }
                """));
}
