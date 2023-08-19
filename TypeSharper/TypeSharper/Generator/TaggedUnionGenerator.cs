using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public class TaggedUnionGenerator : TypeGenerator
{
    public override TsAttrDef AttributeDefinition(IncrementalGeneratorInitializationContext context)
        => new(
            new TsId("TypeSharperTaggedUnionAttribute"),
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
                                                new TsParam(
                                                    TsTypeRef.WithNs("System", "String"),
                                                    $"caseName_{parameterIdx}"))
                                        .Append(
                                            new TsParam(
                                                TsTypeRef.WithNsArray("System", "String"),
                                                "additionalSimpleCases",
                                                true))),
                                TsList.Create<TsParam>(),
                                TsList.Create(
                                    Enumerable
                                        .Range(0, parameterCount)
                                        .Select(parameterIdx => new TsId($"TCaseType_{parameterIdx}")))))));

    public override bool RunDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType targetType,
        TsAttr attr)
        => Diag.RunTypeIsAbstractDiagnostics(sourceProductionContext, targetType);

    #region Protected

    protected override TsModel DoGenerate(TsType targetType, TsAttr attr, TsModel model)
        => TsType
           .CreateTaggedUnion(
               targetType.Info,
               attr,
               TsList.Create(
                   attr
                       .FlattenedArgs()
                       .Zip(
                           attr
                               .TypeArgs
                               .Select(Maybe<TsTypeRef>.Some)
                               .Concat(Maybe<TsTypeRef>.NONE.Repeat()),
                           (caseName, caseValueType)
                               => new TsType.TaggedUnion.Case(caseName, caseValueType))))
           .Aggregate(
               model,
               (acc, type) => acc.AddType(type));

    #endregion
}
