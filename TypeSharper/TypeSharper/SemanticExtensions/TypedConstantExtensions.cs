using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypeSharper.SemanticExtensions;

public static class TypedConstantExtensions
{
    public static string[] ToValue(this TypedConstant constant)
        => constant switch
        {
            { Kind: TypedConstantKind.Primitive, Type.SpecialType: SpecialType.System_String, Value: not null } =>
                new[] { constant.Value.ToString() },
            { Kind: TypedConstantKind.Array } =>
                constant.Values.SelectMany(el => el.ToValue()).ToArray(),
            _ => throw new NotSupportedException("Only strings or (nested) arrays of strings are supported"),
        };
}
