using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    #region Nested types

    public record TypeInfo(TsId Id, Maybe<TsTypeRef> ContainingType, TsNs Ns)
    {
        public TsTypeRef Ref()
            => ContainingType.Match(
                ct => ct.AddId(Id),
                () => TsTypeRef.WithNs(Ns.Id, Id));
    }

    #endregion
}
