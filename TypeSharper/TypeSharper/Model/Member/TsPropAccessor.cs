using System;
using TypeSharper.Model.Modifier;
using TypeSharper.Support;

namespace TypeSharper.Model.Member;

public record TsPropAccessor(TsPropAccessor.EKind Kind, ETsVisibility Visibility)
{
    public static TsPropAccessor PublicGet() => new(EKind.Get, ETsVisibility.Public);
    public static TsPropAccessor PublicInit() => new(EKind.Init, ETsVisibility.Public);

    public string Cs()
        => Visibility.Cs(ETsVisibility.Public).MarginRight()
           + Kind switch
           {
               EKind.Get  => "get;",
               EKind.Set  => "set;",
               EKind.Init => "init;",
               _          => throw new ArgumentOutOfRangeException(),
           };

    public override string ToString() => Cs();

    #region Equality Members

    public virtual bool Equals(TsPropAccessor? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Kind == other.Kind
               && Visibility == other.Visibility;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)Kind * 397) ^ (int)Visibility;
        }
    }

    #endregion

    #region Nested types

    public enum EKind
    {
        Get, Set, Init,
    }

    #endregion
}
