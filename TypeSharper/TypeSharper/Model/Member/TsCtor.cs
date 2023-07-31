using TypeSharper.Model.Identifier;
using TypeSharper.Support;

namespace TypeSharper.Model.Member;

public record TsCtor(
        TsList<TsParam> Params,
        TsMemberMods Mods,
        Maybe<string> CsBody)
    : TsMember(Mods)
{
    public string Cs(TsId typeId)
        => $"{Mods.Cs()} {typeId.Cs()}({CsParameters()}){CsBody.Match(csBody => $"\n{csBody}", () => ";")}";

    public override string ToString() => Cs(new TsId("__CTOR__"));

    #region Private

    private string CsParameters() => Params.Select(param => param.Cs()).JoinList();

    #endregion

    #region Equality Members

    public virtual bool Equals(TsCtor? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other)
               && Params.Equals(other.Params)
               && CsBody.Equals(other.CsBody);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ Params.GetHashCode();
            hashCode = (hashCode * 397) ^ CsBody.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
