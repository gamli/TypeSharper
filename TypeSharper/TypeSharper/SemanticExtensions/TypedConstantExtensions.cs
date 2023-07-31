using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model.Attr;

namespace TypeSharper.SemanticExtensions;

public static class TypedConstantExtensions
{
    public static TsAttrValue ToAttrValue(this TypedConstant constant)
        => constant.Kind switch
        {
            TypedConstantKind.Primitive
                when constant.Type?.SpecialType == SpecialType.System_String
                     && constant.Value != null =>
                TsAttrValue.Primitive(constant.Value.ToString()),
            TypedConstantKind.Array =>
                TsAttrValue.Array(constant.Values.Select(el => el.ToAttrValue().AssertPrimitive())),
            _ => throw new NotSupportedException("Only strings or arrays of strings are supported"),
        };
}
