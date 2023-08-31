using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;

namespace TypeSharper.SemanticExtensions;

public static class AttributeDataExtensions
{
    public static bool IsTsAttribute(this AttributeData attributeData)
        => attributeData
           .AttributeClass
           ?.ContainingNamespace
           .ToDisplayString()
           .StartsWith(TsAttr.NS)
           ?? false;

    public static Maybe<TsAttr> ToAttr(this AttributeData attributeData)
    {
        var attributeClass = attributeData.AttributeClass;

        if (attributeClass == null)
        {
            throw new ArgumentNullException(nameof(attributeData), "At " + nameof(attributeData.AttributeClass));
        }

        var attributeTypeRef = attributeClass.ToTypeRef();

        var ctorArgs =
            TsUniqueList.Create(attributeData.ConstructorArguments.SelectMany(arg => arg.ToValue()));

        var namedArgs =
            attributeData
                .NamedArguments
                .ToDictionary(
                    kv => new TsName(kv.Key),
                    kv => kv.Value.ToValue());

        var typeArgs =
            TsUniqueList.Create(
                attributeClass
                    .TypeArguments
                    .Select(typeArg => typeArg.ToTypeRef()));

        return TypeSharperAttributes.MatchAttributes<TsAttr>(
            attributeTypeRef.Name.Cs(),
            ()
                => new TsType.IntersectionAttr(typeArgs),
            ()
                => new TsType.OmittedAttr(
                    typeArgs.Single(),
                    TsUniqueList.CreateRange(ctorArgs.Select(arg => new TsName(arg)))),
            ()
                => new TsType.PickedAttr(
                    typeArgs.Single(),
                    TsUniqueList.CreateRange(ctorArgs.Select(arg => new TsName(arg)))),
            ()
                => new TsType.ProductAttr(typeArgs),
            ()
                => new TsType.TaggedUnionAttr(
                    TsUniqueList.CreateRange(ctorArgs.Select(arg => new TsName(arg))),
                    TsList.CreateRange(typeArgs)));
    }
}
