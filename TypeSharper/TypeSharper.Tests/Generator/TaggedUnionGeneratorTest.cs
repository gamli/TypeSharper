using TypeSharper.Diagnostics;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class UnionGeneratorTests
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
            public abstract partial class UnionTarget { }
            """,
            // language=csharp
            """
            public abstract partial class UnionTarget
            """,
            // language=csharp
            """
            private UnionTarget()
            """,
            // language=csharp
            """
            public static UnionTarget CreateIFC(ICaseInterface value)
                => new IFC(value);
            """,
            // language=csharp
            """
            public static UnionTarget CreateCLS(CaseClass value)
                => new CLS(value);
            """,
            // language=csharp
            """
            public static UnionTarget CreateREC(CaseRecord value)
                => new REC(value);
            """,
            // language=csharp
            """
            public static UnionTarget CreateSCT(CaseStruct value)
                => new SCT(value);
            """,
            // language=csharp
            """
            public static UnionTarget CreatePRIM(System.String value)
                => new PRIM(value);
            """,
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
            """
            private sealed class CLS : UnionTarget
            {
                public CLS(CaseClass value)
                    => Value = value;
            
                public CaseClass Value { get; }
            }
            """,
            // language=csharp
            """
            private sealed class IFC : UnionTarget
            {
                public IFC(ICaseInterface value)
                    => Value = value;
            
                public ICaseInterface Value { get; }
            }
            """,
            // language=csharp
            """
            private sealed class PRIM : UnionTarget
            {
                public PRIM(System.String value)
                    => Value = value;
            
                public System.String Value { get; }
            }
            """,
            // language=csharp
            """
            private sealed class REC : UnionTarget
            {
                public REC(CaseRecord value)
                    => Value = value;
                
                public CaseRecord Value { get; }
            }
            """,
            // language=csharp
            """
            private sealed class SCT : UnionTarget
            {
                public SCT(CaseStruct value)
                    => Value = value;
            
                public CaseStruct Value { get; }
            }
            """);

    [Fact]
    public void Union_cases_are_not_required_to_have_a_value()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperTaggedUnion<string>("StringCase", "EmptyCase")]
            public abstract partial class UnionWithEmptyCase { }
            """,
            // language=csharp
            """
            public abstract partial class UnionWithEmptyCase
            """,
            // language=csharp
            """
            private UnionWithEmptyCase()
            """,
            // language=csharp
            """
            public static UnionWithEmptyCase CreateStringCase(System.String value)
                => new StringCase(value);
            """,
            // language=csharp
            """
            public static UnionWithEmptyCase CreateEmptyCase()
                => new EmptyCase();
            """,
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
            """
            private sealed class EmptyCase : UnionWithEmptyCase { }
            """,
            // language=csharp
            """
            private sealed class StringCase : UnionWithEmptyCase
            {
                public StringCase(System.String value)
                    => Value = value;
            
                public System.String Value { get; }
            }
            """);

    [Fact]
    public void Union_of_multiple_primitive_types()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperTaggedUnion<string, int, object>("StringCase", "IntCase", "AnObjectCase")]
            public abstract partial class OneOfStringIntObject { }
            """,
            // language=csharp
            """
            public abstract partial class OneOfStringIntObject
            """,
            // language=csharp
            """
            private OneOfStringIntObject()
            """,
            // language=csharp
            """
            public static OneOfStringIntObject CreateStringCase(System.String value)
                => new StringCase(value);
            """,
            // language=csharp
            """
            public static OneOfStringIntObject CreateIntCase(System.Int32 value)
                => new IntCase(value);
            """,
            // language=csharp
            """
            public static OneOfStringIntObject CreateAnObjectCase(System.Object value)
                => new AnObjectCase(value);
            """,
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
            """
            private sealed class AnObjectCase : OneOfStringIntObject
            {
                public AnObjectCase(System.Object value)
                    => Value = value;
            
                public System.Object Value { get; }
            }
            """,
            // language=csharp
            """
            private sealed class IntCase : OneOfStringIntObject
            {
                public IntCase(System.Int32 value)
                    => Value = value;
            
                public System.Int32 Value { get; }
            }
            """,
            // language=csharp
            """
            private sealed class StringCase : OneOfStringIntObject
            {
                public StringCase(System.String value)
                    => Value = value;
            
                public System.String Value { get; }
            }
            """);

    [Fact]
    public void Union_target_type_must_be_abstract()
        => GeneratorTest.Fail(
            EDiagnosticsCode.TypeMustBeAbstract,
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperTaggedUnion<string, int, object>("s", "i", "o")]
            public partial class OneOfStringIntObject { }
            """);
}
