namespace TypeSharper.Model.Modifier;

public record TsTargetTypeMod(bool IsSet)
{
    #region Equality Members

    public virtual bool Equals(TsTargetTypeMod? other)
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
