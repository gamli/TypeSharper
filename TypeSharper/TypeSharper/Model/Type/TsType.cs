using System;
using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;

namespace TypeSharper.Model.Type;

public record TsType(
    TsId Id,
    Maybe<TsTypeRef> BaseType,
    Maybe<TsTypeRef> ContainingType,
    TsNs Ns,
    TsType.EKind TypeKind,
    TsTypeMods Mods,
    Maybe<TsPrimaryCtor> PrimaryCtor,
    TsList<TsCtor> Ctors,
    TsList<TsProp> Props,
    TsList<TsMethod> Methods,
    TsList<TsAttr> Attrs)
{
    public TsType(
        TsId Id,
        Maybe<TsTypeRef> BaseType,
        Maybe<TsTypeRef> ContainingType,
        TsNs Ns,
        EKind TypeKind,
        TsTypeMods Mods)
        : this(
            Id,
            BaseType,
            ContainingType,
            Ns,
            TypeKind,
            Mods,
            Maybe.None<TsPrimaryCtor>(),
            TsList<TsCtor>.Empty,
            TsList<TsProp>.Empty,
            TsList<TsMethod>.Empty,
            TsList<TsAttr>.Empty) { }


    public TsType AddAttr(TsAttr attr) => AddAttrs(attr);
    public TsType AddAttrs(params TsAttr[] attrs) => AddAttrs((IEnumerable<TsAttr>)attrs);
    public TsType AddAttrs(IEnumerable<TsAttr> attrs) => this with { Attrs = Attrs.AddRange(attrs) };

    public TsType AddCtor(TsCtor ctor) => AddCtors(ctor);
    public TsType AddCtors(params TsCtor[] ctors) => AddCtors((IEnumerable<TsCtor>)ctors);
    public TsType AddCtors(IEnumerable<TsCtor> ctors) => this with { Ctors = Ctors.AddRange(ctors) };

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


    public TsType AddProp(TsProp prop) => AddProps(prop);
    public TsType AddProps(params TsProp[] props) => AddProps((IEnumerable<TsProp>)props);
    public TsType AddProps(IEnumerable<TsProp> props) => this with { Props = Props.AddRange(props) };

    public string Cs(TsModel model) => $"{CsNs()}{CsAttrs().AddRightIfNotEmpty("\n")}{CsSignature()}{CsBody(model)}";

    public TsType Diff(TsType otherType)
        => NewPartial()
           .SetPrimaryCtor(PrimaryCtor.IfNone(() => otherType.PrimaryCtor))
           .AddMembers(
               otherType.Ctors.Where(ctor => !Ctors.Contains(ctor)),
               otherType.Props.Where(prop => !Props.Contains(prop)),
               otherType.Methods.Where(method => !Methods.Contains(method)),
               otherType.Attrs.Where(attr => !Attrs.Contains(attr)));

    public bool IsTopLevel() => ContainingType.Match(_ => false, () => true);

    public TsType Merge(TsType type)
        => type.PrimaryCtor.Match(
            ctor => SetPrimaryCtor(ctor).AddMembers(type.Ctors, type.Props, type.Methods, type.Attrs),
            () => AddMembers(type.Ctors, type.Props, type.Methods, type.Attrs));

    public TsType NewPartial()
        => this with
        {
            BaseType = Maybe.None<TsTypeRef>(),
            Ctors = TsList<TsCtor>.Empty,
            Props = TsList<TsProp>.Empty,
            Methods = TsList<TsMethod>.Empty,
            Attrs = TsList<TsAttr>.Empty,
        };

    public TsTypeRef Ref() => new(Ns, ContainingType.Match(typeRef => typeRef.Id.Add(Id), () => new TsQualifiedId(Id)));
    public TsType RemovePrimaryCtor() => this with { PrimaryCtor = Maybe.None<TsPrimaryCtor>() };

    public TsType SetPrimaryCtor(TsPrimaryCtor primaryCtor) => SetPrimaryCtor(Maybe.Some(primaryCtor));
    public TsType SetPrimaryCtor(Maybe<TsPrimaryCtor> primaryCtor) => this with { PrimaryCtor = primaryCtor };

    #region Private

    private string CsAttrs() => Attrs.Select(attr => attr.Cs()).JoinLines();
    private string CsBaseType() => BaseType.Match(type => type.Cs(), () => "");

    private string CsBody(TsModel model)
    {
        var primaryConstructor = CsPrimaryCtor();
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
            ? primaryConstructor.Match(
                ctor => $"{ctor};",
                () => TypeKind is EKind.RecordClass or EKind.RecordStruct ? ";" : " { }")
            : primaryConstructor.Match(
                ctor => $$"""
                    {{ctor}}
                    {
                    {{lines.JoinLines().Indent()}}
                    }
                    """,
                () => $$"""
                    {
                    {{lines.JoinLines().Indent()}}
                    }
                    """);
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
        => new[] { CsCtors(Id), CsProps(), CsMethods(), CsNestedTypes(model) }.Flatten();

    private IEnumerable<string> CsMethods() => Methods.Select(method => method.Cs());

    private IEnumerable<string> CsNestedTypes(TsModel model)
        => model.NestedTypes(this).Select(nestedType => nestedType.Cs(model));

    private string CsNs() => ContainingType.Match(_ => "", () => Ns.Cs().AddRightIfNotEmpty("\n\n"));

    private Maybe<string> CsPrimaryCtor() => PrimaryCtor.IfSome(ctor => ctor.Cs());

    private IEnumerable<string> CsProps()
        => PrimaryCtor.Match(
            ctor =>
            {
                var ctorParams = new HashSet<TsId>(ctor.Params.Select(param => param.Id));
                return Props.Where(prop => !ctorParams.Contains(prop.Id)).Select(prop => prop.Cs());
            },
            () => Props.Select(prop => prop.Cs()));

    private string CsSignature()
        => $"{Mods.Cs().MarginRight()}{CsKind()} {Id.Cs()}{CsBaseType().AddLeftIfNotEmpty(": ")}";

    #endregion

    #region Nested types

    public enum EKind
    {
        Interface,
        Class,
        RecordClass,
        Struct,
        RecordStruct,
        Special,
    }

    #endregion
}
