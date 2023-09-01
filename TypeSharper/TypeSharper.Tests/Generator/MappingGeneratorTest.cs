using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class MappingGeneratorTests
{
    [Fact]
    public void Mapping_is_part_of_all_duck_property_types_and_can_be_used_in_combination()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public record MappingSource(string Name, bool IsSample, int Count);
            [TsOmitAttribute<MappingSource>(
                "Name",
                "Count",
                Mappings = new[] { "IsSample", "System.String" })]
            public partial record MappingTarget;
            """,
            // language=csharp
            "public partial record MappingTarget(System.String IsSample)");

    [Fact]
    public void Mapping_multiple_types_is_possible_by_passing_a_flat_list_of_string_pairs_containing_name_and_type()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public record MappingSource(string Name, bool IsSample, int Count);
            [TsOmitAttribute<MappingSource>(
                Mappings = new[] {
                    "Name", "System.Boolean",
                    "IsSample", "System.String",
                    "Count", "System.Object",
                })]
            public partial record MappingTarget;
            """,
            // language=csharp
            "public partial record MappingTarget(System.Boolean Name, System.String IsSample, System.Object Count)");

    [Fact]
    public void The_declared_mappings_map_the_type_of_a_property_to_another_type()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public record MappingSource(string Name, bool IsSample, int Count);
            [TsOmitAttribute<MappingSource>(Mappings = new[] { "Name", "System.Boolean" })]
            public partial record MappingTarget;
            """,
            // language=csharp
            "public partial record MappingTarget(System.Boolean Name, System.Boolean IsSample, System.Int32 Count)");

    [Fact]
    public void The_mapped_type_will_not_be_sanitized_to_be_fully_qualified_as_usual_but_used_as_is()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            public record MappingSource(string Name);
            [TsOmitAttribute<MappingSource>(Mappings = new[] { "Name", "bool" })]
            public partial record MappingTarget;
            """,
            // language=csharp
            "public partial record MappingTarget(bool Name)");

    [Fact]
    public void Trying_to_map_a_property_that_does_not_exist_is_an_error()
        => GeneratorTest.Fail(
            EDiagnosticsCode.MappedPropertyDoesNotExist,
            // ReSharper disable once HeapView.ObjectAllocation
            // language=csharp
            """
            using TypeSharper.Attributes;
            public record MappingSource(string Name);
            [TsOmitAttribute<MappingSource>(Mappings = new[] { "PropThatDoesNotExist", "System.Boolean" })]
            public partial record MappingTarget;
            """);
}
