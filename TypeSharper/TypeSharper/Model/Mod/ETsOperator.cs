using System;

namespace TypeSharper.Model.Mod;

public enum ETsOperator
{
    Implicit, Explicit, None,
}

public static class ETsOperatorExtensions
{
    public static string Cs(this ETsOperator op)
        => op switch
        {
            ETsOperator.Implicit => "implicit operator",
            ETsOperator.Explicit => "explicit operator",
            ETsOperator.None     => "",
            _                    => throw new ArgumentOutOfRangeException(nameof(op), op, null),
        };
}
