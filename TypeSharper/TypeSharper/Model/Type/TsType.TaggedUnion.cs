using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    public static IEnumerable<TsType> CreateTaggedUnion(
        TypeInfo info,
        TsAttr typeSharperAttr,
        TsList<TaggedUnion.Case> cases)
        => TaggedUnionImpl.Create(info, typeSharperAttr, cases);

    #region Nested types

    public interface TaggedUnion : Duck
    {
        TsList<Case> Cases { get; }

        #region Nested types

        public record Case(TsId Name, Maybe<TsTypeRef> ValueType)
        {
            public TsTypeRef TypeRef() => TsTypeRef.WithoutNs(Name);
        }

        #endregion
    }

    private sealed record TaggedUnionImpl : DuckImpl, TaggedUnion
    {
        public TsList<TaggedUnion.Case> Cases { get; }

        public static IEnumerable<TsType> Create(
            TypeInfo info,
            TsAttr typeSharperAttr,
            TsList<TaggedUnion.Case> cases)
        {
            var containerType = ContainerType(info, typeSharperAttr, cases);
            return CreateCaseTypes(containerType, cases).Append(containerType);
        }

        public override string ToString() => $"{base.ToString()}:TaggedUnion";

        #region Private

        private TaggedUnionImpl(
            TypeInfo info,
            TsAttr typeSharperAttr,
            TsList<TaggedUnion.Case> cases,
            TsList<TsTypeRef> dependentTypes)
            : base(info, typeSharperAttr, dependentTypes)
            => Cases = cases;

        private static TsType ContainerType(TypeInfo info, TsAttr typeSharperAttr, TsList<TaggedUnion.Case> cases)
            => new TaggedUnionImpl(
                   info,
                   typeSharperAttr,
                   cases,
                   TsList.Create(cases.SelectWhereSome(c => c.ValueType).Distinct()))
               .SetMods(TsTypeMods.PublicPartialAbstract)
               .AddCtor(
                   new TsCtor(
                       TsList.Create<TsParam>(),
                       new TsMemberMods(
                           ETsVisibility.Private,
                           new TsAbstractMod(false),
                           new TsStaticMod(false),
                           ETsOperator.None),
                       "{ }"))
               // .AddMethods(cases.Select(c => FactoryMethod(info, c)))
               .AddMethods(
                   MatchMethod(cases, Maybe<TsTypeRef>.NONE),
                   MatchMethod(cases, TsTypeRef.WithoutNs("TResult")));

        private static IEnumerable<TsType> CreateCaseTypes(TsType containerType, TsList<TaggedUnion.Case> cases)
            => cases.Select(
                t =>
                {
                    var (caseName, caseValueType) = t;
                    var caseType = CreateNative(
                        new TypeInfo(caseName, containerType.Ref(), containerType.Info.Ns),
                        containerType.Ref(),
                        EKind.RecordClass,
                        TsTypeMods.PublicSealed);
                    return caseValueType.Match(
                        valueType => caseType.AddPrimaryCtorPropAndParam(valueType, "Value"),
                        () => caseType);
                });


        private static string CsMatchBody(
            TsList<TaggedUnion.Case> namesAndTypes,
            Maybe<TsTypeRef> maybeReturnType)
        {
            var matchCases =
                namesAndTypes
                    .Select(
                        t =>
                            t.ValueType.Match(
                                _ =>
                                    maybeReturnType.Match(
                                        _ => $$"""
                                            {{t.Name.Cs()}} c => handle{{t.Name.Capitalize().Cs()}}(c.Value),
                                            """,
                                        () => $$"""
                                            case {{t.Name.Cs()}} c:
                                                handle{{t.Name.Capitalize().Cs()}}(c.Value);
                                                break;
                                            """),
                                () =>
                                    maybeReturnType.Match(
                                        _ => $$"""
                                            {{t.Name.Cs()}} => handle{{t.Name.Capitalize().Cs()}}(),
                                            """,
                                        () => $$"""
                                            case {{t.Name.Cs()}}:
                                                handle{{t.Name.Capitalize().Cs()}}();
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

        private static TsList<TsParam> CsMatchParameters(
            TsList<TaggedUnion.Case> namesAndTypes,
            Maybe<TsTypeRef> maybeReturnType)
            => TsList.Create<TsParam>(
                namesAndTypes.Select<TsParam>(
                    t
                        => new TsParam(
                            TsTypeRef.WithNs(
                                "System",
                                t.ValueType.Match(
                                    valueType
                                        => maybeReturnType.Match(
                                            returnType => $"Func<{valueType.Cs()}, {returnType.Cs()}>",
                                            () => $"Action<{valueType.Cs()}>"),
                                    ()
                                        => maybeReturnType.Match(
                                            returnType => $"Func<{returnType.Cs()}>",
                                            () => "Action"))),
                            $"handle{t.Name.Capitalize().Cs()}")));

        private static TsMethod FactoryMethod(TypeInfo containerTypeInfo, TaggedUnion.Case c)
            => c.ValueType.Match(
                valueType =>
                    TsMethod.Factory(
                        c.Name.Cs(),
                        containerTypeInfo.Ref(),
                        valueType,
                        param => $"=> new {c.Name.Cs()}({param.Id.Cs()});".Indent()),
                () => TsMethod.Factory(
                    c.Name.Cs(),
                    containerTypeInfo.Ref(),
                    $"=> new {c.Name.Cs()}();".Indent()));

        private static TsMethod MatchMethod(
            TsList<TaggedUnion.Case> namesAndTypes,
            Maybe<TsTypeRef> maybeReturnType)
            => new(
                new TsId("Match"),
                maybeReturnType.Match(
                    returnType => returnType,
                    () => TsTypeRef.WithoutNs("void")),
                maybeReturnType.Match(returnType => TsList.Create(returnType), () => new TsList<TsTypeRef>()),
                CsMatchParameters(namesAndTypes, maybeReturnType),
                new TsMemberMods(
                    ETsVisibility.Public,
                    new TsAbstractMod(false),
                    new TsStaticMod(false),
                    ETsOperator.None),
                CsMatchBody(namesAndTypes, maybeReturnType));

        #endregion
    }

    #endregion
}
