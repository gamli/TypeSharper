using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;
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
                                                    $"caseName_{parameterIdx}",
                                                    false))
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
    {
        var caseInfos = CaseInfos(targetType, attr);

        var ctor =
            new TsCtor(
                TsList.Create<TsParam>(),
                new TsMemberMods(
                    ETsVisibility.Private,
                    new TsAbstractMod(false),
                    new TsStaticMod(false),
                    ETsOperator.None),
                "{ }");

        var factoryMethods = caseInfos.Select(FactoryMethod);

        var matchMethods = new[]
        {
            CsMatchMethod(caseInfos, Maybe<TsTypeRef>.NONE),
            CsMatchMethod(
                caseInfos,
                Maybe<TsTypeRef>.Some(TsTypeRef.WithoutNs("TResult"))),
        };

        var caseTypes = caseInfos.Select(CaseType);

        return caseTypes
               .Append(
                   targetType
                       .NewPartial()
                       .AddCtor(ctor)
                       .AddMethods(factoryMethods)
                       .AddMethods(matchMethods))
               .Aggregate(model, (generatedModel, type) => generatedModel.AddType(type));
    }

    #endregion

    #region Private

    private static TsList<CaseInfo> CaseInfos(TsType targetType, TsAttr attr)
        => TsList.Create(
            attr
                .CtorArgs
                .SkipLast()
                .Zip(
                    attr
                        .TypeArgs
                        .Select(Maybe<TsTypeRef>.Some)
                        .Concat(Maybe<TsTypeRef>.NONE.Repeat()),
                    (caseName, caseType) =>
                        new CaseInfo(
                            new TsId(caseName.AssertPrimitive()),
                            caseType,
                            targetType))
                .Concat(
                    attr
                        .CtorArgs
                        .Last()
                        .AssertArray()
                        .Select(
                            caseWithoutValueName =>
                                new CaseInfo(new TsId(caseWithoutValueName), Maybe<TsTypeRef>.NONE, targetType))));

    private static TsType CaseType(CaseInfo caseInfo)
    {
        var caseTypeMods = new TsTypeMods(
            ETsVisibility.Private,
            new TsAbstractMod(false),
            new TsStaticMod(false),
            new TsSealedMod(true),
            new TsPartialMod(false),
            new TsTargetTypeMod(false));

        var caseType =
            new TsType(
                caseInfo.CaseName,
                caseInfo.TargetType.Ref(),
                caseInfo.TargetType.Ref(),
                caseInfo.TargetType.Ns,
                caseInfo.TargetType.TypeKind,
                caseTypeMods);

        caseInfo.CaseType.IfSome(
            innerCaseType =>
            {
                caseType =
                    caseType
                        .SetPrimaryCtor(TsPrimaryCtor.Create(new TsParam(innerCaseType, "Value", false)))
                        .AddProp(
                            new TsProp(
                                innerCaseType,
                                new TsId("Value"),
                                new TsMemberMods(
                                    ETsVisibility.Public,
                                    new TsAbstractMod(false),
                                    new TsStaticMod(false),
                                    ETsOperator.None),
                                TsProp.BodyImpl.Accessors(TsList.Create(TsPropAccessor.PublicGet()))));
            });

        return caseType;
    }

    private static string CsMatchBody(
        TsList<CaseInfo> caseNamesAndTypes,
        Maybe<TsTypeRef> maybeReturnType)
    {
        var matchCases =
            caseNamesAndTypes
                .Select(
                    t =>
                        t.CaseType.Match(
                            _ =>
                                maybeReturnType.Match(
                                    _ => $$"""
                                        {{t.CaseName.Cs()}} c => handle{{t.CaseName.Capitalize().Cs()}}(c.Value),
                                        """,
                                    () => $$"""
                                        case {{t.CaseName.Cs()}} c:
                                            handle{{t.CaseName.Capitalize().Cs()}}(c.Value);
                                            break;
                                        """),
                            () =>
                                maybeReturnType.Match(
                                    _ => $$"""
                                        {{t.CaseName.Cs()}} c => handle{{t.CaseName.Capitalize().Cs()}}(),
                                        """,
                                    () => $$"""
                                        case {{t.CaseName.Cs()}}:
                                            handle{{t.CaseName.Capitalize().Cs()}}();
                                            break;
                                        """)))
                .JoinLines();
        return maybeReturnType.Match(
            _ => $$"""
                => this switch
                   {
                {{matchCases.Indent(3).Indent()}}
                   };
                """,
            () => $$"""
                {
                    switch(this)
                    {
                {{matchCases.Indent().Indent()}}
                    }
                }
                """);
    }

    private static TsMethod CsMatchMethod(TsList<CaseInfo> caseNamesAndTypes, Maybe<TsTypeRef> maybeReturnType)
        => new(
            new TsId("Match"),
            maybeReturnType.Match(
                returnType => returnType,
                () => TsTypeRef.WithoutNs("void")),
            maybeReturnType.Match(returnType => TsList.Create(returnType), () => new TsList<TsTypeRef>()),
            CsMatchParameters(caseNamesAndTypes, maybeReturnType),
            new TsMemberMods(
                ETsVisibility.Public,
                new TsAbstractMod(false),
                new TsStaticMod(false),
                ETsOperator.None),
            CsMatchBody(caseNamesAndTypes, maybeReturnType));

    private static TsList<TsParam> CsMatchParameters(
        TsList<CaseInfo> caseNamesAndTypes,
        Maybe<TsTypeRef> maybeReturnType)
        => TsList.Create<TsParam>(
            caseNamesAndTypes.Select<TsParam>(
                t
                    => new TsParam(
                        TsTypeRef.WithNs(
                            "System",
                            t.CaseType.Match(
                                caseType
                                    => maybeReturnType.Match(
                                        returnType => $"Func<{caseType.Cs()}, {returnType.Cs()}>",
                                        () => $"Action<{caseType.Cs()}>"),
                                ()
                                    => maybeReturnType.Match(
                                        returnType => $"Func<{returnType.Cs()}>",
                                        () => "Action"))),
                        $"handle{t.CaseName.Capitalize().Cs()}",
                        false)));

    private static TsMethod FactoryMethod(CaseInfo caseInfo)
    {
        var name = new TsId($"Create{caseInfo.CaseName.Capitalize().Cs()}");
        var returnType = caseInfo.TargetType.Ref();
        var mods = new TsMemberMods(
            ETsVisibility.Public,
            new TsAbstractMod(false),
            new TsStaticMod(true),
            ETsOperator.None);

        return caseInfo.CaseType.Match(
            caseType =>
                new TsMethod(
                    name,
                    returnType,
                    new TsList<TsTypeRef>(),
                    TsList.Create(new TsParam(caseType, new TsId("value"), false)),
                    mods,
                    $"    => new {caseInfo.CaseName.Cs()}(value);"),
            () => new TsMethod(
                name,
                returnType,
                new TsList<TsTypeRef>(),
                TsList.Create<TsParam>(),
                mods,
                $"    => new {caseInfo.CaseName.Cs()}();"));
    }

    #endregion

    #region Nested types

    private record CaseInfo(TsId CaseName, Maybe<TsTypeRef> CaseType, TsType TargetType);

    #endregion
}
