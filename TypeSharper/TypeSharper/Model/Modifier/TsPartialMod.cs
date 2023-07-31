namespace TypeSharper.Model.Modifier;

public record TsPartialMod(bool IsSet)
{
    public string Cs() => IsSet ? "partial" : "";
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsPartialMod? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsSet == other.IsSet;
    }

    public override int GetHashCode() => IsSet.GetHashCode();

    #endregion
}
