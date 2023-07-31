using TypeSharper.Model.Modifier;
using TypeSharper.Support;

namespace TypeSharper.Model.Type;

public record TsTypeMods(
    ETsVisibility Visibility,
    TsAbstractMod Abstract,
    TsStaticMod Static,
    TsSealedMod Sealed,
    TsPartialMod Partial,
    TsTargetTypeMod TargetType)
{
    public string Cs() => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs(), Sealed.Cs(), Partial.Cs() }.JoinTokens();
    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsTypeMods? other)
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
               && Static.Equals(other.Static)
               && Sealed.Equals(other.Sealed)
               && Partial.Equals(other.Partial)
               && TargetType.Equals(other.TargetType);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)Visibility;
            hashCode = (hashCode * 397) ^ Abstract.GetHashCode();
            hashCode = (hashCode * 397) ^ Static.GetHashCode();
            hashCode = (hashCode * 397) ^ Sealed.GetHashCode();
            hashCode = (hashCode * 397) ^ Partial.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetType.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
