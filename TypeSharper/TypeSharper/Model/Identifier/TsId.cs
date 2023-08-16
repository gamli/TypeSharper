namespace TypeSharper.Model.Identifier;

public record TsId(string Value)
{
    public TsId Capitalize() => new(Cs().Capitalize());

    public string Cs() => Value;
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsId? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    #endregion
}
