using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class TypeGeneratorTests
{
    [Fact]
    public void All_types_in_a_nested_type_hierarchy_must_be_partial()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TypeHierarchyMustBePartial,
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
    public void Each_type_is_generated_into_its_own_file()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public interface IPickSource
            {
                public string Name { get; set; }
            }
            [TypeSharperPickAttribute<IPickSource>("Name")]
            public partial interface IFirstPickTarget {}
            [TypeSharperPickAttribute<IPickSource>("Name")]
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

    [Fact]
    public void Target_type_can_be_class()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public class PickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>("Name", "IsSample")]
            public partial class PickTarget { }
            """,
            // language=csharp
            """
            public partial class PickTarget
            {
                public PickTarget(PickSource fromValue)
                {
                    Name = fromValue.Name;
                    IsSample = fromValue.IsSample;
                }
                    
                public System.String Name { get; set; }
                public System.Boolean IsSample { get; set; }
            }
            """);

    [Fact]
    public void Target_type_can_be_interface()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
                public bool IsSample { get; set; }
            }
            [TypeSharper.Attributes.TypeSharperPickAttribute<IPickSource>("Name", "IsSample")]
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
    public void Target_type_can_be_record()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record PickSource(string Name, bool IsSample);
            [TypeSharper.Attributes.TypeSharperPickAttribute<PickSource>("Name", "IsSample")]
            public partial record PickTarget;
            """,
            // language=csharp
            "public partial record PickTarget(System.String Name, System.Boolean IsSample)");

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
            EDiagnosticsCode.TypeHierarchyMustBePartial,
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
