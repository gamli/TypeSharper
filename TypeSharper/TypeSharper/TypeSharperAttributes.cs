using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.AttrDef;

namespace TypeSharper;

public static class TypeSharperAttributes
{
    private const string _INTERSECTION_ATTRIBUTE_NAME = "TsIntersectionAttribute";
    private const string _OMIT_ATTRIBUTE_NAME = "TsOmitAttribute";
    private const string _PICK_ATTRIBUTE_NAME = "TsPickAttribute";
    private const string _TAGGED_UNION_ATTRIBUTE_NAME = "TsTaggedUnionAttribute";

    public static Maybe<T> MatchAttributes<T>(
        string attribute,
        Func<T> handleIntersection,
        Func<T> handleOmit,
        Func<T> handlePick,
        Func<T> handleTaggedUnion)
        => attribute switch
        {
            _ when attribute.EndsWith(_INTERSECTION_ATTRIBUTE_NAME) => handleIntersection(),
            _ when attribute.EndsWith(_OMIT_ATTRIBUTE_NAME)         => handleOmit(),
            _ when attribute.EndsWith(_PICK_ATTRIBUTE_NAME)         => handlePick(),
            _ when attribute.EndsWith(_TAGGED_UNION_ATTRIBUTE_NAME) => handleTaggedUnion(),
            _                                                       => Maybe<T>.NONE,
        };

    public static IEnumerable<TsAttrDef> Attributes()
        => new[]
        {
            PickAttributeDefinition(),
            OmitAttributeDefinition(),
            IntersectionAttributeDefinition(),
            TaggedUnionAttributeDefinition(),
        };

    private static TsAttrDef IntersectionAttributeDefinition()
        => new(
            new TsName(_INTERSECTION_ATTRIBUTE_NAME),
            AttributeTargets.Class | AttributeTargets.Struct,
            TsList.Create(
                Enumerable
                    .Range(1, 10)
                    .Select(
                        parameterCount =>
                            new TsAttrOverloadDef(
                                new TsList<TsAttrOverloadDef.Param>(),
                                new TsList<TsAttrOverloadDef.Param>(),
                                TsList.Create(
                                    Enumerable
                                        .Range(0, parameterCount)
                                        .Select(parameterIdx => new TsName($"TType_{parameterIdx}")))))));

    private static TsAttrDef OmitAttributeDefinition() => MemberSelectionAttributeDefinition(_OMIT_ATTRIBUTE_NAME);
    private static TsAttrDef PickAttributeDefinition() => MemberSelectionAttributeDefinition(_PICK_ATTRIBUTE_NAME);

    private static TsAttrDef MemberSelectionAttributeDefinition(string attributeName)
        => new(
            attributeName,
            AttributeTargets.Class | AttributeTargets.Struct,
            TsList.Create(
                new TsAttrOverloadDef(
                    TsList.Create(
                        new TsAttrOverloadDef.Param(
                            TsTypeRef.WithNsArray("System", "String"),
                            "memberNames",
                            true)),
                    TsList<TsAttrOverloadDef.Param>.Empty,
                    TsList.Create(new TsName("TFromType")))));

    private static TsAttrDef TaggedUnionAttributeDefinition()
        => new(
            new TsName(_TAGGED_UNION_ATTRIBUTE_NAME),
            AttributeTargets.Class | AttributeTargets.Struct,
            TsList.Create(
                Enumerable
                    .Range(1, 10)
                    .Select(
                        parameterCount =>
                            new TsAttrOverloadDef(
                                TsList.Create(
                                    Enumerable
                                        .Range(0, parameterCount)
                                        .Select(
                                            parameterIdx =>
                                                new TsAttrOverloadDef.Param(
                                                    TsTypeRef.WithNs("System", "String"),
                                                    $"caseName_{parameterIdx}"))
                                        .Append(
                                            new TsAttrOverloadDef.Param(
                                                TsTypeRef.WithNsArray("System", "String"),
                                                "additionalSimpleCases",
                                                true))),
                                TsList.Create<TsAttrOverloadDef.Param>(),
                                TsList.Create(
                                    Enumerable
                                        .Range(0, parameterCount)
                                        .Select(parameterIdx => new TsName($"TCaseType_{parameterIdx}")))))));
}
