using System;

namespace TypeSharper.Model.Modifier;

public enum ETsVisibility
{
    Private,
    Protected,
    Internal,
    Public,
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
