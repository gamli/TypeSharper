using Xunit;

// ReSharper disable HeapView.ObjectAllocation

namespace TypeSharper.Tests.Generator;

public class UnionGeneratorTests
{
    [Fact]
    public void Union_cases_do_not_have_to_have_a_value()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperUnion<string>("StringCase", "EmptyCase")]
            public abstract partial class UnionWithEmptyCase { }
            """,
            // language=csharp
            """
            public abstract partial class UnionWithEmptyCase
            {
                private UnionWithEmptyCase()
                { }
            
                public static UnionWithEmptyCase CreateStringCase(System.String value)
                    => new StringCase(value);
            
                public static UnionWithEmptyCase CreateEmptyCase()
                    => new EmptyCase();
                
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
            
                private sealed class EmptyCase : UnionWithEmptyCase { }
                private sealed class StringCase : UnionWithEmptyCase
                {
                    public StringCase(System.String value)
                        => Value = value;
            
                    public System.String Value { get; }
                }
            }
            """);

    [Fact]
    public void Union_of_multiple_primitive_types()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperUnion<string, int, object>("StringCase", "IntCase", "AnObjectCase")]
            public abstract partial class OneOfStringIntObject { }
            """,
            // language=csharp
            """
            public abstract partial class OneOfStringIntObject
            {
                private OneOfStringIntObject()
                { }
            
                public static OneOfStringIntObject CreateStringCase(System.String value)
                    => new StringCase(value);
            
                public static OneOfStringIntObject CreateIntCase(System.Int32 value)
                    => new IntCase(value);
            
                public static OneOfStringIntObject CreateAnObjectCase(System.Object value)
                    => new AnObjectCase(value);
                
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
            
                private sealed class AnObjectCase : OneOfStringIntObject
                {
                    public AnObjectCase(System.Object value)
                        => Value = value;
            
                    public System.Object Value { get; }
                }
            
                private sealed class IntCase : OneOfStringIntObject
                {
                    public IntCase(System.Int32 value)
                        => Value = value;
            
                    public System.Int32 Value { get; }
                }
            
                private sealed class StringCase : OneOfStringIntObject
                {
                    public StringCase(System.String value)
                        => Value = value;
            
                    public System.String Value { get; }
                }
            }
            """);

    [Fact]
    public void Union_target_type_must_be_abstract()
        => GeneratorTest.Fail(
            // language=csharp
            """
            using TypeSharper.Attributes;
            [TypeSharperUnion<string, int, object>("s", "i", "o")]
            public partial class OneOfStringIntObject { }
            """);

    [Fact]
    public void Unions_of_classes_interfaces_and_primitives_are_supported()
        => GeneratorTest.ExpectOutput(
            // language=csharp
            """
            using TypeSharper.Attributes;

            public interface ICaseInterface {}
            public class CaseClass {}

            [TypeSharperUnion<ICaseInterface, CaseClass, string>("IFC", "CLS", "PRIM")]
            public abstract partial class OneOfStringIntObject { }
            """,
            // language=csharp
            """
            public abstract partial class OneOfStringIntObject
            {
                private OneOfStringIntObject()
                { }
            
                public static OneOfStringIntObject CreateIFC(ICaseInterface value)
                    => new IFC(value);
            
                public static OneOfStringIntObject CreateCLS(CaseClass value)
                    => new CLS(value);
            
                public static OneOfStringIntObject CreatePRIM(System.String value)
                    => new PRIM(value);
            
                public void Match(System.Action<ICaseInterface> handleIFC, System.Action<CaseClass> handleCLS, System.Action<System.String> handlePRIM)
                {
                    switch (this)
                    {
                        case IFC c:
                            handleIFC(c.Value);
                            break;
                        case CLS c:
                            handleCLS(c.Value);
                            break;
                        case PRIM c:
                            handlePRIM(c.Value);
                            break;
                    }
                }
                
                private sealed class CLS : OneOfStringIntObject
                {
                    public CLS(CaseClass value)
                        => Value = value;
            
                    public CaseClass Value { get; }
                }
                
                private sealed class IFC : OneOfStringIntObject
                {
                    public IFC(ICaseInterface value)
                        => Value = value;
            
                    public ICaseInterface Value { get; }
                }
            
                private sealed class PRIM : OneOfStringIntObject
                {
                    public PRIM(System.String value)
                        => Value = value;
            
                    public System.String Value { get; }
                }
            }
            """);
}
