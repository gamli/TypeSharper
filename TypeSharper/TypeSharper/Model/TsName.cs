using System;

namespace TypeSharper.Model;

public record TsName(string Value) : IComparable<TsName>
{
    public static implicit operator TsName(string value) => new(value);
    public TsName Capitalize() => new(Cs().Capitalize());

    public string Cs() => Value;
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsName? other)
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

    public int CompareTo(TsName other)
        => string.Compare(Value, other.Value, StringComparison.InvariantCultureIgnoreCase);

    public override int GetHashCode() => Value.GetHashCode();

    #endregion
}
