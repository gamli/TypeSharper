using System;
using Microsoft.CodeAnalysis;
using TypeSharper.Model.Modifier;

namespace TypeSharper.SemanticExtensions;

public static class AccessibilityExtensions
{
    public static ETsVisibility ToVisibility(this Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.NotApplicable => ETsVisibility.Public,
            Accessibility.Private => ETsVisibility.Private,
            Accessibility.ProtectedAndInternal => ETsVisibility.Internal,
            Accessibility.Protected => ETsVisibility.Protected,
            Accessibility.Internal => ETsVisibility.Internal,
            Accessibility.ProtectedOrInternal => ETsVisibility.Internal,
            Accessibility.Public => ETsVisibility.Public,
            _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null),
        };
}
