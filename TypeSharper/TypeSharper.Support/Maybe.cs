using System;

namespace TypeSharper;

public abstract record Maybe<T>
{
    public static readonly Maybe<T> NONE = new NoneCase();
    public static implicit operator Maybe<T>(T value) => Some(value);

    public static Maybe<T> Some(T value) => new SomeCase(value);

    public Maybe<TResult> IfNone<TResult>(Func<TResult> what) => this is NoneCase ? what() : Maybe<TResult>.NONE;
    public Maybe<TResult> IfNone<TResult>(Func<Maybe<TResult>> what) => this is NoneCase ? what() : Maybe<TResult>.NONE;

    public void IfNone(Action what)
    {
        if (this == NONE)
        {
            what();
        }
    }

    public Maybe<TResult> IfSome<TResult>(Func<T, TResult> what)
        => this is SomeCase some ? what(some.Value) : Maybe<TResult>.NONE;

    public Maybe<TResult> IfSome<TResult>(Func<T, Maybe<TResult>> what)
        => this is SomeCase some ? what(some.Value) : Maybe<TResult>.NONE;

    public void IfSome(Action<T> what)
    {
        if (this is SomeCase some)
        {
            what(some.Value);
        }
    }

    public TResult Match<TResult>(Func<T, TResult> ifSome, Func<TResult> ifNone)
        => this is SomeCase some ? ifSome(some.Value) : ifNone();

    public Maybe<TResult> Match<TResult>(Func<T, Maybe<TResult>> ifSome, Func<Maybe<TResult>> ifNone)
        => this is SomeCase some ? ifSome(some.Value) : ifNone();

    #region Private

    private Maybe() { }

    #endregion

    #region Nested types

    private sealed record NoneCase : Maybe<T>;

    private sealed record SomeCase(T Value) : Maybe<T>;

    #endregion
}
