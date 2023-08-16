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
            AttributeTargets.Interface | AttributeTargets.Class,
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
                                                    new TsTypeRef(
                                                        TsNs.Qualified(new TsQualifiedId("System")),
                                                        new TsQualifiedId("String")),
                                                    new TsId($"caseName_{parameterIdx}"),
                                                    false))
                                        .Append(
                                            new TsParam(
                                                new TsTypeRef(
                                                    TsNs.Qualified(new TsQualifiedId("System")),
                                                    new TsQualifiedId("String"),
                                                    true),
                                                new TsId("additionalSimpleCases"),
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
                Maybe.Some("{ }"));

        var factoryMethods = caseInfos.Select(FactoryMethod);

        var matchMethod = MatchMethod(caseInfos);

        var caseTypes = caseInfos.Select(CaseType);

        return caseTypes
               .Append(
                   targetType
                       .NewPartial()
                       .AddCtor(ctor)
                       .AddMethods(factoryMethods)
                       .AddMethod(matchMethod))
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
                        .Select(Maybe.Some)
                        .Concat(Maybe.None<TsTypeRef>().Repeat()),
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
                                new CaseInfo(new TsId(caseWithoutValueName), Maybe.None<TsTypeRef>(), targetType))));

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
                Maybe.Some(caseInfo.TargetType.Ref()),
                Maybe.Some(caseInfo.TargetType.Ref()),
                caseInfo.TargetType.Ns,
                caseInfo.TargetType.TypeKind,
                caseTypeMods);

        caseInfo.CaseType.IfSome(
            innerCaseType =>
            {
                caseType =
                    caseType
                        .AddCtor(
                            new TsCtor(
                                TsList.Create(new TsParam(innerCaseType, new TsId("value"), false)),
                                new TsMemberMods(
                                    ETsVisibility.Public,
                                    new TsAbstractMod(false),
                                    new TsStaticMod(false),
                                    ETsOperator.None),
                                Maybe.Some("    => Value = value;")))
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
                    TsList.Create(new TsParam(caseType, new TsId("value"), false)),
                    mods,
                    Maybe.Some($"    => new {caseInfo.CaseName.Cs()}(value);")),
            () => new TsMethod(
                name,
                returnType,
                TsList.Create<TsParam>(),
                mods,
                Maybe.Some($"    => new {caseInfo.CaseName.Cs()}();")));
    }

    private static TsMethod MatchMethod(TsList<CaseInfo> caseNamesAndTypes)
    {
        var name = new TsId("Match");

        var returnType = new TsTypeRef(TsNs.Global, new TsQualifiedId("void"));

        var parameters =
            TsList.Create<TsParam>(
                caseNamesAndTypes.Select<TsParam>(
                    t
                        => new TsParam(
                            new TsTypeRef(
                                TsNs.Qualified(new TsQualifiedId("System")),
                                new TsQualifiedId(
                                    t.CaseType.Match(
                                        caseType => $"Action<{caseType.Cs()}>",
                                        () => "Action"))),
                            new TsId($"handle{t.CaseName.Capitalize().Cs()}"),
                            false)));

        var matchCases =
            caseNamesAndTypes
                .Select(
                    t =>
                        t.CaseType.Match(
                            _ => $$"""
                                case {{t.CaseName.Cs()}} c:
                                    handle{{t.CaseName.Capitalize().Cs()}}(c.Value);
                                    break;
                                """,
                            () => $$"""
                                case {{t.CaseName.Cs()}}:
                                    handle{{t.CaseName.Capitalize().Cs()}}();
                                    break;
                                """))
                .JoinLines();

        var mods = new TsMemberMods(
            ETsVisibility.Public,
            new TsAbstractMod(false),
            new TsStaticMod(false),
            ETsOperator.None);

        var bodySrc =
            Maybe.Some(
                $$"""
                {
                    switch(this)
                    {
                {{matchCases.Indent().Indent()}}
                    }
                }
                """);

        return new TsMethod(name, returnType, parameters, mods, bodySrc);
    }

    #endregion

    #region Nested types

    private record CaseInfo(TsId CaseName, Maybe<TsTypeRef> CaseType, TsType TargetType);

    #endregion
}
