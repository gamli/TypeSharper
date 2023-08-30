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
                    [TypeSharper.Attributes.TsPickAttribute<IPickSource>(nameof(PickSource.Name))]
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
            [TsPickAttribute<IPickSource>("Name")]
            public partial record FirstPickTarget {}
            [TsPickAttribute<IPickSource>("Name")]
            public partial record SecondPickTarget {}
            """,
            ("FirstPickTarget.g.cs",
                // language=csharp
                "public partial record FirstPickTarget(System.String Name)"),
            ("SecondPickTarget.g.cs",
                // language=csharp
                "public partial record SecondPickTarget(System.String Name)"));

    [Fact]
    public void Target_type_can_NOT_be_class()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TargetTypeMustBeRecord,
            // language=csharp
            """
            public class PickSource
            {
                public string Name { get; set; }
            }
            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Name")]
            public partial class PickTargetClass { }
            """);

    [Fact]
    public void Target_type_can_NOT_be_interface()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TargetTypeMustBeRecord,
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
            }
            [TypeSharper.Attributes.TsPickAttribute<IPickSource>("Name")]
            public partial interface IPickTargetInterface { }
            """);

    [Fact]
    public void Target_type_can_NOT_be_struct()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TargetTypeMustBeRecord,
            // language=csharp
            """
            public struct PickSource
            {
                public string Name { get; set; }
            }
            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Name")]
            public partial struct PickTargetStruct { }
            """);

    [Fact]
    public void Target_type_MUST_be_record()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public record PickSource(string Name, bool IsSample);

            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Name", "IsSample")]
            public partial record PickTargetRecordDefault;

            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Name", "IsSample")]
            public partial record class PickTargetRecordClass;

            [TypeSharper.Attributes.TsPickAttribute<PickSource>("Name", "IsSample")]
            public partial record struct PickTargetRecordStruct;
            """,
            ("PickTargetRecordDefault.g.cs",
                // language=csharp
                "public partial record PickTargetRecordDefault(System.String Name, System.Boolean IsSample)"),
            ("PickTargetRecordClass.g.cs",
                // language=csharp
                "public partial record PickTargetRecordClass(System.String Name, System.Boolean IsSample)"),
            ("PickTargetRecordStruct.g.cs",
                // language=csharp
                "public sealed partial record struct PickTargetRecordStruct(System.String Name, System.Boolean IsSample)"));

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
                [TypeSharper.Attributes.TsPickAttribute<IPickSource>(nameof(PickSource.Name))]
                public partial record NestedPickTarget;
            }
            """,
            // language=csharp
            """
            public partial interface IPickTarget
            {
                public partial record NestedPickTarget(System.String Name)
            """);

    [Fact]
    public void Targeting_a_non_partial_type_generates_an_error()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TypeHierarchyMustBePartial,
            // language=csharp
            """
            public class PickSource { }
            [TypeSharper.Attributes.TsPickAttribute<PickSource>(nameof(PickSource.Count))]
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
            [TypeSharper.Attributes.TsPickAttribute<IPickSource>(nameof(PickSource.Name))]
            public partial record PickTarget;
            """,
            // language=csharp
            "namespace MyNs;",
            // language=csharp
            "public partial record PickTarget(System.String Name)");

    [Fact]
    public void Using_a_generated_type_in_another_generated_type_is_possible()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            public interface IPickSource
            {
                public string Name { get; set; }
            }
            [TypeSharper.Attributes.TsPickAttribute<IPickSource>(nameof(IPickSource.Name))]
            public partial record FirstPickTarget;
            [TypeSharper.Attributes.TsPickAttribute<FirstPickTarget>(nameof(FirstPickTarget.Name))]
            public partial record SecondPickTarget;
            """,
            ("FirstPickTarget.g.cs",
                // language=csharp
                "public partial record FirstPickTarget(System.String Name)"),
            ("SecondPickTarget.g.cs",
                // language=csharp
                "public partial record SecondPickTarget(System.String Name)"));
}
