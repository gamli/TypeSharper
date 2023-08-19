using TypeSharper.Model.Attr;
using TypeSharper.Model.Member;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    #region Nested types

    public interface Duck : TsType
    {
        public TsList<TsTypeRef> DependentTypes { get; }
    }

    private abstract record DuckImpl(
            TypeInfo Info,
            TsAttr TypeSharperAttr,
            TsList<TsTypeRef> DependentTypes,
            Maybe<TsPrimaryCtor> PrimaryCtor,
            TsList<TsCtor> Ctors,
            TsList<TsProp> Props,
            TsList<TsMethod> Methods)
        : Base(
              Info,
              Maybe<TsTypeRef>.NONE,
              EKind.RecordClass,
              TsTypeMods.PublicPartial,
              PrimaryCtor,
              Ctors,
              Props,
              Methods,
              TsList.Create(TypeSharperAttr)),
          Duck
    {
        #region Protected

        protected DuckImpl(TypeInfo Info, TsAttr TypeSharperAttr, TsList<TsTypeRef> DependentTypes) : this(
            Info,
            TypeSharperAttr,
            DependentTypes,
            Maybe<TsPrimaryCtor>.NONE,
            TsList<TsCtor>.Empty,
            TsList<TsProp>.Empty,
            TsList<TsMethod>.Empty) { }

        #endregion
    }

    #endregion
}
