using System;

namespace TypeSharper;

public static class ControlFlowExtensions
{
    public static T If<T>(this T value, bool condition, Func<T> handleTrue) => condition ? handleTrue() : value;

    public static TResult IfElse<T, TResult>(
        this T _,
        bool condition,
        Func<TResult> handleTrue,
        Func<TResult> handleFalse)
        => condition ? handleTrue() : handleFalse();
}
