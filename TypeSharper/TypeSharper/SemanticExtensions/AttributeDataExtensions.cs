using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;

namespace TypeSharper.SemanticExtensions;

public static class AttributeDataExtensions
{
    public static bool IsTsAttribute(this AttributeData attributeData)
        => attributeData
           .AttributeClass
           ?.ContainingNamespace
           .ToDisplayString()
           .StartsWith(TsAttr.ATTRIBUTE_NAMESPACE_ID.Cs())
           ?? false;

    public static TsAttr ToAttr(this AttributeData attributeData)
    {
        var attributeClass = attributeData.AttributeClass;

        if (attributeClass == null)
        {
            throw new ArgumentNullException(nameof(attributeData), "At " + nameof(attributeData.AttributeClass));
        }

        var attributeTypeRef = attributeClass.ToTypeRef();

        var ctorArgs =
            TsList.Create(attributeData.ConstructorArguments.Select(arg => arg.ToAttrValue()));

        var namedArgs =
            attributeData
                .NamedArguments
                .ToDictionary(
                    kv => new TsId(kv.Key),
                    kv => kv.Value.ToAttrValue());

        var typeArgs =
            TsList.Create(
                attributeClass
                    .TypeArguments
                    .Select(typeArg => typeArg.ToTypeRef()));

        return new TsAttr(attributeTypeRef, attributeData.IsTsAttribute(), ctorArgs, namedArgs, typeArgs);
    }
}
