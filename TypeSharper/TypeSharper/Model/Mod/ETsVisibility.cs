using System;

namespace TypeSharper.Model.Mod;

public enum ETsVisibility
{
    Private = 3,
    Protected = 2,
    Internal = 1,
    Public = 0,
}

public static class ETsVisibilityExtensions
{
    public static string Cs(this ETsVisibility visibility, ETsVisibility? ignoredVisibility = null)
        => visibility == ignoredVisibility
            ? ""
            : visibility switch
            {
                ETsVisibility.Private   => "private",
                ETsVisibility.Protected => "protected",
                ETsVisibility.Internal  => "internal",
                ETsVisibility.Public    => "public",
                _                       => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null),
            };
}
