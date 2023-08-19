using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class TaggedUnionGeneratorTests
{
    [Fact]
    public void Union_case_values_can_be_of_any_kind()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;

            public interface ICaseInterface {}
            public class CaseClass {}
            public record CaseRecord;
            public struct CaseStruct;

            [TypeSharperTaggedUnion<ICaseInterface, CaseClass, CaseRecord, CaseStruct, string>("IFC", "CLS", "REC", "SCT", "PRIM")]
            public abstract partial record UnionTarget;
            """,
            // language=csharp
            "public abstract partial record UnionTarget",
            // language=csharp
            "private UnionTarget()",
            // language=csharp
            """
            public void Match(
                System.Action<ICaseInterface> handleIFC,
                System.Action<CaseClass> handleCLS,
                System.Action<CaseRecord> handleREC,
                System.Action<CaseStruct> handleSCT,
                System.Action<System.String> handlePRIM)
            {
                switch (this)
                {
                    case IFC c:
                        handleIFC(c.Value);
                        break;
                    case CLS c:
                        handleCLS(c.Value);
                        break;
                    case REC c:
                        handleREC(c.Value);
                        break;
                    case SCT c:
                        handleSCT(c.Value);
                        break;
                    case PRIM c:
                        handlePRIM(c.Value);
                        break;
                }
            }
            """,
            // language=csharp
            "public sealed record CLS(CaseClass Value) : UnionTarget;",
            // language=csharp
            "public sealed record IFC(ICaseInterface Value) : UnionTarget;",
            // language=csharp
            "public sealed record PRIM(System.String Value) : UnionTarget;",
            // language=csharp
            "public sealed record REC(CaseRecord Value) : UnionTarget;",
            // language=csharp
            "public sealed record SCT(CaseStruct Value) : UnionTarget;");

    [Fact]
    public void Union_cases_are_not_required_to_have_a_value()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperTaggedUnion<string>("StringCase", "EmptyCase")]
            public abstract partial record UnionWithEmptyCase;
            """,
            // language=csharp
            "public abstract partial record UnionWithEmptyCase",
            // language=csharp
            "private UnionWithEmptyCase()",
            // language=csharp
            """
            public void Match(System.Action<System.String> handleStringCase, System.Action handleEmptyCase)
            {
                switch (this)
                {
                    case StringCase c:
                        handleStringCase(c.Value);
                        break;
                    case EmptyCase:
                        handleEmptyCase();
                        break;
                }
            }
            """,
            // language=csharp
            "public sealed record EmptyCase : UnionWithEmptyCase;",
            // language=csharp
            "public sealed record StringCase(System.String Value) : UnionWithEmptyCase;");

    [Fact]
    public void Union_of_multiple_primitive_types()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperTaggedUnion<string, int, object>("StringCase", "IntCase", "AnObjectCase")]
            public abstract partial record OneOfStringIntObject;
            """,
            // language=csharp
            "public abstract partial record OneOfStringIntObject",
            // language=csharp
            "private OneOfStringIntObject()",
            // language=csharp
            """
            public void Match(
                System.Action<System.String> handleStringCase,
                System.Action<System.Int32> handleIntCase,
                System.Action<System.Object> handleAnObjectCase)
            {
                switch (this)
                {
                    case StringCase c:
                        handleStringCase(c.Value);
                        break;
                    case IntCase c:
                        handleIntCase(c.Value);
                        break;
                    case AnObjectCase c:
                        handleAnObjectCase(c.Value);
                        break;
                }
            }
            """,
            // language=csharp
            "public sealed record AnObjectCase(System.Object Value) : OneOfStringIntObject;",
            // language=csharp
            "public sealed record IntCase(System.Int32 Value) : OneOfStringIntObject;",
            // language=csharp
            "public sealed record StringCase(System.String Value) : OneOfStringIntObject;");

    [Fact]
    public void Union_target_type_must_be_abstract()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TypeMustBeAbstract,
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperTaggedUnion<string, int, object>("s", "i", "o")]
            public partial record OneOfStringIntObject { }
            """);
}
