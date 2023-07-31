using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsParam(TsTypeRef Type, TsId Id, bool IsParams)
{
    public string Cs() => $"{(IsParams ? "params " : "")}{Type.Cs()} {Id.Cs()}";
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsParam? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Type.Equals(other.Type)
               && Id.Equals(other.Id)
               && IsParams == other.IsParams;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Type.GetHashCode();
            hashCode = (hashCode * 397) ^ Id.GetHashCode();
            hashCode = (hashCode * 397) ^ IsParams.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
