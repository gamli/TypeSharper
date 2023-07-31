using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;
using TypeSharper.Support;

namespace TypeSharper.Model.Member;

public record TsMethod(
        TsId Id,
        TsTypeRef ReturnType,
        TsList<TsParam> Params,
        TsMemberMods Mods,
        Maybe<string> CsBody)
    : TsMember(Mods)
{
    public string Cs()
        => $"{Mods.Cs()} {ReturnType.Cs()} {Id.Cs()}({CsParameters()}){CsBody.Match(csBody => $"\n{csBody}", () => ";")}";

    public override string ToString() => Cs();

    #region Private

    private string CsParameters() => Params.Select(p => p.Cs()).JoinList();

    #endregion

    #region Equality Members

    public virtual bool Equals(TsMethod? other)
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
               && Id.Equals(other.Id)
               && ReturnType.Equals(other.ReturnType)
               && Params.Equals(other.Params)
               && CsBody.Equals(other.CsBody);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ Id.GetHashCode();
            hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
            hashCode = (hashCode * 397) ^ Params.GetHashCode();
            hashCode = (hashCode * 397) ^ CsBody.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
