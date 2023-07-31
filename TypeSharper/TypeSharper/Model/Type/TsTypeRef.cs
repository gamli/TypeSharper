using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public record TsTypeRef(TsNs Ns, TsQualifiedId Id, bool IsArray = false)
{
    public string Cs() => Ns.Match(nsId => $"{nsId.Cs()}.{Id.Cs()}", () => Id.Cs()) + (IsArray ? "[]" : "");

    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsTypeRef? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Ns.Equals(other.Ns)
               && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Ns.GetHashCode() * 397) ^ Id.GetHashCode();
        }
    }

    #endregion
}
