using TypeSharper.Model.Attr;
using TypeSharper.Model.Member;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    public static TsType CreateNative(
        TypeInfo info,
        Maybe<TsTypeRef> baseType,
        EKind typeKind,
        TsTypeMods mods)
        => new NativeImpl(info, baseType, typeKind, mods);

    #region Nested types

    private sealed record NativeImpl(
            TypeInfo Info,
            Maybe<TsTypeRef> BaseType,
            EKind TypeKind,
            TsTypeMods Mods)
        : Base(
            Info,
            BaseType,
            TypeKind,
            Mods,
            Maybe<TsPrimaryCtor>.NONE,
            TsList<TsCtor>.Empty,
            TsList<TsProp>.Empty,
            TsList<TsMethod>.Empty,
            TsList<TsAttr>.Empty)
    {
        public override string ToString() => $"{base.ToString()}:Native";
    }

    #endregion
}
