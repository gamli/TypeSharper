using System;
using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    #region Nested types

    private record Base(
        TypeInfo Info,
        Maybe<TsTypeRef> BaseType,
        EKind TypeKind,
        TsTypeMods Mods,
        Maybe<TsPrimaryCtor> PrimaryCtor,
        TsList<TsCtor> Ctors,
        TsList<TsProp> Props,
        TsList<TsMethod> Methods,
        TsList<TsAttr> Attrs) : TsType
    {
        public bool SupportsPrimaryCtor => TypeKind is EKind.RecordClass or EKind.RecordStruct;
        public TsType AddAttr(TsAttr attr) => AddAttrs(attr);
        public TsType AddAttrs(params TsAttr[] attrs) => AddAttrs((IEnumerable<TsAttr>)attrs);
        public TsType AddAttrs(IEnumerable<TsAttr> attrs) => this with { Attrs = Attrs.AddRange(attrs) };

        public TsType AddCastOperator(TsTypeRef fromType, Func<TsParam, string> csBody, ETsOperator op)
            => AddMethod(TsMethod.CastOperator(fromType, Ref(), csBody, op));

        public TsType AddCtor(TsCtor ctor) => AddCtors(ctor);
        public TsType AddCtors(params TsCtor[] ctors) => AddCtors((IEnumerable<TsCtor>)ctors);
        public TsType AddCtors(IEnumerable<TsCtor> ctors) => this with { Ctors = Ctors.AddRange(ctors) };

        public TsType AddExplicitCastOperator(TsTypeRef fromType, Func<TsParam, string> csBody)
            => AddMethod(TsMethod.ExplicitCastOperator(fromType, Ref(), csBody));

        public TsType AddImplicitCastOperator(TsTypeRef fromType, Func<TsParam, string> csBody)
            => AddMethod(TsMethod.ImplicitCastOperator(fromType, Ref(), csBody));

        public TsType AddMembers(
            IEnumerable<TsCtor> ctors,
            IEnumerable<TsProp> props,
            IEnumerable<TsMethod> methods,
            IEnumerable<TsAttr> attrs)
            => this with
            {
                Ctors = Ctors.AddRange(ctors),
                Props = Props.AddRange(props),
                Methods = Methods.AddRange(methods),
                Attrs = Attrs.AddRange(attrs),
            };

        public TsType AddMethod(TsMethod method) => AddMethods(method);
        public TsType AddMethods(params TsMethod[] methods) => AddMethods((IEnumerable<TsMethod>)methods);
        public TsType AddMethods(IEnumerable<TsMethod> methods) => this with { Methods = Methods.AddRange(methods) };

        public TsType AddPrimaryCtorParam(TsId propId)
            => SetPrimaryCtor(PrimaryCtor.Match(ctor => ctor.PropAndParamIds.Append(propId), () => new[] { propId }));

        public TsType AddPrimaryCtorPropAndParam(TsTypeRef type, TsId id)
            => AddProp(TsProp.RecordPrimaryCtorProp(type, id))
                .AddPrimaryCtorParam(id);

        public TsType AddProp(TsProp prop) => AddProps(prop);
        public TsType AddProps(params TsProp[] props) => AddProps((IEnumerable<TsProp>)props);
        public TsType AddProps(IEnumerable<TsProp> props) => this with { Props = Props.AddRange(props) };

        public TsType AddPublicCtor(Maybe<string> csBody, params TsParam[] parameters)
            => AddPublicCtor(csBody, (IEnumerable<TsParam>)parameters);

        public TsType AddPublicCtor(Maybe<string> csBody, IEnumerable<TsParam> parameters)
            => AddCtor(TsCtor.Public(csBody, parameters));

        public string Cs(TsModel model)
            => $"{CsNs()}{CsAttrs().AddRightIfNotEmpty("\n")}{CsSignature()}{CsBody(model)}";

        public TsType Diff(TsType otherType)
            => (this with
               {
                   BaseType = Maybe<TsTypeRef>.NONE,
                   Ctors = TsList<TsCtor>.Empty,
                   Props = TsList<TsProp>.Empty,
                   Methods = TsList<TsMethod>.Empty,
                   Attrs = TsList<TsAttr>.Empty,
               })
               .SetPrimaryCtor(PrimaryCtor.IfNone(() => otherType.PrimaryCtor))
               .AddMembers(
                   otherType.Ctors.Where(ctor => !Ctors.Contains(ctor)),
                   otherType.Props.Where(prop => !Props.Contains(prop)),
                   otherType.Methods.Where(method => !Methods.Contains(method)),
                   otherType.Attrs.Where(attr => !Attrs.Contains(attr)));

        public Maybe<T> IfDuck<T>(Func<Duck, T> handleDuck) => this is DuckImpl duck ? handleDuck(duck) : Maybe<T>.NONE;

        public Maybe<T> IfIntersection<T>(Func<Intersection, T> handleIntersection)
            => this is IntersectionImpl intersection ? handleIntersection(intersection) : Maybe<T>.NONE;

        public Maybe<T> IfNative<T>(Func<TsType, T> handleNative)
            => this is NativeImpl native ? handleNative(native) : Maybe<T>.NONE;

        public Maybe<T> IfOmitted<T>(Func<Omitted, T> handleOmitted)
            => this is OmittedImpl omitted ? handleOmitted(omitted) : Maybe<T>.NONE;

        public Maybe<T> IfPicked<T>(Func<Picked, T> handlePicked)
            => this is PickedImpl picked ? handlePicked(picked) : Maybe<T>.NONE;

        public Maybe<T> IfTaggedUnion<T>(Func<TaggedUnion, T> handleTaggedUnion)
            => this is TaggedUnionImpl taggedUnion ? handleTaggedUnion(taggedUnion) : Maybe<T>.NONE;

        public bool IsTopLevel() => Info.ContainingType.Match(_ => false, () => true);

        public T Match<T>(
            Func<TsType, T> handleNative,
            Func<Picked, T> handlePicked,
            Func<Omitted, T> handleOmitted,
            Func<Intersection, T> handleIntersection,
            Func<TaggedUnion, T> handleTaggedUnion)
            => this switch
            {
                NativeImpl native             => handleNative(native),
                PickedImpl picked             => handlePicked(picked),
                OmittedImpl omitted           => handleOmitted(omitted),
                IntersectionImpl intersection => handleIntersection(intersection),
                TaggedUnionImpl taggedUnion   => handleTaggedUnion(taggedUnion),
                _                             => throw new ArgumentOutOfRangeException(),
            };

        public T MatchNativeOrDuck<T>(Func<TsType, T> handleNative, Func<Duck, T> handleDuck)
            => this switch
            {
                Duck duck         => handleDuck(duck),
                NativeImpl native => handleNative(native),
                _                 => throw new ArgumentOutOfRangeException(),
            };

        public TsType Merge(TsType type)
            => type.PrimaryCtor.Match(
                ctor => SetPrimaryCtor(ctor).AddMembers(type.Ctors, type.Props, type.Methods, type.Attrs),
                () => AddMembers(type.Ctors, type.Props, type.Methods, type.Attrs));

        public TsProp Prop(TsId propId) => Props.Single(prop => prop.Id == propId);


        public TsTypeRef Ref()
            => Info.ContainingType.Match(
                typeRef => typeRef.AddId(Info.Id),
                () => TsTypeRef.WithNs(Info.Ns.Id, Info.Id));

        public TsType RemovePrimaryCtor() => this with { PrimaryCtor = Maybe<TsPrimaryCtor>.NONE };


        public TsType SetMods(TsTypeMods mods) => this with { Mods = mods };

        public TsType SetPrimaryCtor(params TsId[] propIds) => SetPrimaryCtor((IEnumerable<TsId>)propIds);

        public TsType SetPrimaryCtor(IEnumerable<TsId> propIds)
            => SetPrimaryCtor(TsPrimaryCtor.Create(TsList.Create(propIds)));

        public TsType SetPrimaryCtor(Maybe<TsPrimaryCtor> primaryCtor)
            => primaryCtor.Match(SetPrimaryCtor, RemovePrimaryCtor);

        public TsType SetPrimaryCtor(TsPrimaryCtor primaryCtor) => this with { PrimaryCtor = primaryCtor };


        public TsParam ToParam(TsId paramId, bool isParams = false) => Ref().ToParam(paramId, isParams);

        public override string ToString() => Ref().ToString();

        #region Private

        private string CsAttrs() => Attrs.Select(attr => attr.Cs()).JoinLines();
        private string CsBaseType() => BaseType.Match(type => type.Cs(), () => "");

        private string CsBody(TsModel model)
        {
            var membersAndNestedTypes = CsMembersAndNestedTypes(model).ToList();
            var lines =
                membersAndNestedTypes
                    .Where(memberOrType => !string.IsNullOrWhiteSpace(memberOrType))
                    .Select(
                        (memberOrType, idx)
                            => idx == membersAndNestedTypes.Count - 1
                                ? memberOrType
                                : memberOrType + (memberOrType.Lines().Length > 1 ? "\n" : ""))
                    .ToList();

            return lines is { Count: 0 }
                ? TypeKind is EKind.RecordClass or EKind.RecordStruct ? ";" : " { }"
                : $$"""
                {
                {{lines.JoinLines().Indent()}}
                }
                """;
        }

        private IEnumerable<string> CsCtors(TsId typeId) => Ctors.Select(ctor => ctor.Cs(typeId));

        private string CsKind()
            => TypeKind switch
            {
                EKind.Interface    => "interface",
                EKind.Class        => "class",
                EKind.RecordClass  => "record",
                EKind.Struct       => "struct",
                EKind.RecordStruct => "record struct",
                _                  => throw new ArgumentOutOfRangeException(nameof(TypeKind), TypeKind, null),
            };

        private IEnumerable<string> CsMembersAndNestedTypes(TsModel model)
            => new[] { CsCtors(Info.Id), CsProps(), CsMethods(), CsNestedTypes(model) }.Flatten();

        private IEnumerable<string> CsMethods() => Methods.Select(method => method.Cs());

        private IEnumerable<string> CsNestedTypes(TsModel model)
            => model.NestedTypes(this).Select(nestedType => nestedType.Cs(model));

        private string CsNs()
            => Info.ContainingType.Match(
                _ => "",
                () => Info.Ns.CsFileScoped().AddRightIfNotEmpty("\n\n"));

        private Maybe<string> CsPrimaryCtor() => PrimaryCtor.IfSome(ctor => ctor.Cs(this));

        private IEnumerable<string> CsProps()
            => PrimaryCtor
               .Match(
                   ctor => Props.Where(prop => !ctor.PropAndParamIds.Contains(prop.Id)),
                   () => Props)
               .Select(prop => prop.Cs());

        private string CsSignature()
        {
            var primaryConstructor = CsPrimaryCtor().Match(ctor => ctor, () => "").MarginRight();
            var modsAndKind = $"{Mods.Cs().MarginRight()}{CsKind()}";
            var baseType = CsBaseType().AddLeftIfNotEmpty(": ");
            return $"{modsAndKind} {Info.Id.Cs()}{primaryConstructor}{baseType}";
        }

        #endregion
    }

    #endregion
}
