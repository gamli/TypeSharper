using System;
using TypeSharper.Model.Modifier;

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

    #region Nested types

    public enum EKind
    {
        Get, Set, Init,
    }

    #endregion
}
