using TypeSharper.Model.Modifier;
using TypeSharper.Support;

namespace TypeSharper.Model.Member;

public record TsMemberMods(ETsVisibility Visibility, TsAbstractMod Abstract, TsStaticMod Static)
{
    public string Cs() => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs() }.JoinTokens();
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsMemberMods? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Visibility == other.Visibility
               && Abstract.Equals(other.Abstract)
               && Static.Equals(other.Static);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)Visibility;
            hashCode = (hashCode * 397) ^ Abstract.GetHashCode();
            hashCode = (hashCode * 397) ^ Static.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
