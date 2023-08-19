using System;
using System.Collections.Generic;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    public string Cs(TsModel model);

    public TsType Diff(TsType otherType);

    public Maybe<T> IfDuck<T>(Func<Duck, T> handleDuck);
    public Maybe<T> IfIntersection<T>(Func<Intersection, T> handleIntersection);
    public Maybe<T> IfNative<T>(Func<TsType, T> handleNative);
    public Maybe<T> IfOmitted<T>(Func<Omitted, T> handleOmitted);
    public Maybe<T> IfPicked<T>(Func<Picked, T> handlePicked);
    public Maybe<T> IfTaggedUnion<T>(Func<TaggedUnion, T> handleTaggedUnion);

    public bool IsTopLevel();

    public T Match<T>(
        Func<TsType, T> handleNative,
        Func<Picked, T> handlePicked,
        Func<Omitted, T> handleOmitted,
        Func<Intersection, T> handleIntersection,
        Func<TaggedUnion, T> handleTaggedUnion);

    public T MatchNativeOrDuck<T>(Func<TsType, T> handleNative, Func<Duck, T> handleDuck);

    TsProp Prop(TsId propId);

    public TsTypeRef Ref();

    public TsParam ToParam(TsId paramId, bool isParams = false);

    #region Type Properties

    public TsList<TsAttr> Attrs { get; }
    public Maybe<TsTypeRef> BaseType { get; }
    public TsList<TsCtor> Ctors { get; }
    public TypeInfo Info { get; }
    public TsList<TsMethod> Methods { get; }
    public TsTypeMods Mods { get; }
    public Maybe<TsPrimaryCtor> PrimaryCtor { get; }
    public TsList<TsProp> Props { get; }
    public EKind TypeKind { get; }
    public bool SupportsPrimaryCtor { get; }

    #endregion

    #region Builder methods

    public TsType Merge(TsType type);

    public TsType RemovePrimaryCtor();

    public TsType SetPrimaryCtor(params TsId[] propIds);

    public TsType SetPrimaryCtor(IEnumerable<TsId> propIds);

    public TsType SetPrimaryCtor(Maybe<TsPrimaryCtor> primaryCtor);

    public TsType SetPrimaryCtor(TsPrimaryCtor primaryCtor);

    public TsType AddAttr(TsAttr attr);
    public TsType AddAttrs(params TsAttr[] attrs);
    public TsType AddAttrs(IEnumerable<TsAttr> attrs);

    public TsType AddCastOperator(TsTypeRef fromType, Func<TsParam, string> csBody, ETsOperator op);

    public TsType AddCtor(TsCtor ctor);
    public TsType AddCtors(params TsCtor[] ctors);
    public TsType AddCtors(IEnumerable<TsCtor> ctors);

    public TsType AddExplicitCastOperator(TsTypeRef fromType, Func<TsParam, string> csBody);

    public TsType AddImplicitCastOperator(TsTypeRef fromType, Func<TsParam, string> csBody);

    TsType AddMembers(
        IEnumerable<TsCtor> ctors,
        IEnumerable<TsProp> props,
        IEnumerable<TsMethod> methods,
        IEnumerable<TsAttr> attrs);

    public TsType AddMethod(TsMethod method);
    public TsType AddMethods(params TsMethod[] methods);
    public TsType AddMethods(IEnumerable<TsMethod> methods);


    public TsType AddPrimaryCtorPropAndParam(TsTypeRef type, TsId id);
    public TsType AddPrimaryCtorParam(TsId propId);
    public TsType AddProp(TsProp prop);
    public TsType AddProps(params TsProp[] props);
    public TsType AddProps(IEnumerable<TsProp> props);

    public TsType AddPublicCtor(Maybe<string> csBody, params TsParam[] parameters);

    public TsType AddPublicCtor(Maybe<string> csBody, IEnumerable<TsParam> parameters);

    #endregion
}
