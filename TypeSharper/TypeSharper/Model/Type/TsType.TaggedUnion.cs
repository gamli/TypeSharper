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
               .AddMethods(
                   MatchMethod(cases, Maybe<TsTypeRef>.NONE),
                   MatchMethod(cases, TsTypeRef.WithoutNs("TResult")))
               .AddMethods(cases.Select(IfMethod));

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

        private static TsList<TsParam> CsMatchParams(TsList<TaggedUnion.Case> cases, Maybe<TsTypeRef> maybeReturnType)
            => TsList.Create<TsParam>(cases.Select<TsParam>(c => CsMatchParam(c, maybeReturnType)));

        private static TsParam CsMatchParam(TaggedUnion.Case c, Maybe<TsTypeRef> maybeReturnType)
            => new(
                TsTypeRef.WithNs(
                    "System",
                    c.ValueType.Match(
                        valueType
                            => maybeReturnType.Match(
                                returnType => $"Func<{valueType.Cs()}, {returnType.Cs()}>",
                                () => $"Action<{valueType.Cs()}>"),
                        ()
                            => maybeReturnType.Match(
                                returnType => $"Func<{returnType.Cs()}>",
                                () => "Action"))),
                CsMatchParamId(c));

        private static TsId CsMatchParamId(TaggedUnion.Case c) => $"handle{c.Name.Capitalize().Cs()}";

        private static TsMethod MatchMethod(
            TsList<TaggedUnion.Case> cases,
            Maybe<TsTypeRef> maybeReturnType)
            => SwitchMethod(
                "Match",
                maybeReturnType,
                maybeReturnType,
                CsMatchBody(cases, maybeReturnType),
                CsMatchParams(cases, maybeReturnType).ToArray());

        private static TsMethod IfMethod(TaggedUnion.Case c)
            => SwitchMethod(
                $"If{c.Name}",
                TsTypeRef.WithNs(Constants.TS_SUPPORT_NAMESPACE, "Maybe<TResult>"),
                TsTypeRef.WithoutNs("TResult"),
                // language=C#
                $$"""
                => this is {{c.Name}}{{c.ValueType.Match(_ => " caseValue", () => "")}}
                   ? {{CsMatchParamId(c)}}({{c.ValueType.Match(_ => "caseValue.Value", () => "")}})
                   : Maybe<TResult>.NONE;
                """,
                CsMatchParam(c, TsTypeRef.WithoutNs("TResult")));


        private static TsMethod SwitchMethod(
            string name,
            Maybe<TsTypeRef> maybeReturnType,
            Maybe<TsTypeRef> maybeTypeParameter,
            string csBody,
            params TsParam[] handlerParameters)
            => new(
                name,
                maybeReturnType.Match(
                    returnType => returnType,
                    () => TsTypeRef.WithoutNs("void")),
                maybeTypeParameter.Match(
                    typeParameter => TsList.Create(typeParameter),
                    () => TsList<TsTypeRef>.Empty),
                TsList.Create(handlerParameters),
                new TsMemberMods(
                    ETsVisibility.Public,
                    new TsAbstractMod(false),
                    new TsStaticMod(false),
                    ETsOperator.None),
                csBody);

        #endregion
    }

    #endregion
}
