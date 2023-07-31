namespace TypeSharper.Model.Member;

public abstract record TsMember(TsMemberMods Mods)
{
    #region Equality Members

    public virtual bool Equals(TsMember? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Mods.Equals(other.Mods);
    }

    public override int GetHashCode() => Mods.GetHashCode();

    #endregion
}
