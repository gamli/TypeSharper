using System;
using System.Collections.Generic;

namespace TypeSharper.Support;

public static class Maybe
{
    public static Maybe<T> None<T>() => Maybe<T>.NONE;
    public static Maybe<T> Some<T>(T value) => Maybe<T>.Some(value);
}

public abstract record Maybe<T>
{
    public static readonly Maybe<T> NONE = new NoneCase();

    public static Maybe<T> Some(T value) => new SomeCase(value);

    public void IfNone(Action what)
    {
        if (this is NoneCase)
        {
            what();
        }
    }

    public Maybe<TResult> IfNone<TResult>(Func<TResult> what)
        => this is NoneCase ? Maybe.Some(what()) : Maybe.None<TResult>();

    public Maybe<TResult> IfNone<TResult>(Func<Maybe<TResult>> what)
        => this is NoneCase ? what() : Maybe.None<TResult>();

    public void IfSome(Action<T> what)
    {
        if (this is SomeCase some)
        {
            what(some.Value);
        }
    }

    public Maybe<TResult> IfSome<TResult>(Func<T, TResult> what)
        => this is SomeCase some ? Maybe.Some(what(some.Value)) : Maybe.None<TResult>();

    public Maybe<TResult> IfSome<TResult>(Func<T, Maybe<TResult>> what)
        => this is SomeCase some ? what(some.Value) : Maybe.None<TResult>();

    public void Match(Action<T> ifSome, Action ifNone)
    {
        if (this is SomeCase some)
        {
            ifSome(some.Value);
        }
        else
        {
            ifNone();
        }
    }

    public TResult Match<TResult>(Func<T, TResult> ifSome, Func<TResult> ifNone)
        => this is SomeCase some ? ifSome(some.Value) : ifNone();

    public Maybe<TResult> Match<TResult>(Func<T, Maybe<TResult>> ifSome, Func<TResult> ifNone)
        => this is SomeCase some ? ifSome(some.Value) : Maybe.Some(ifNone());

    public Maybe<TResult> Match<TResult>(Func<T, TResult> ifSome, Func<Maybe<TResult>> ifNone)
        => this is SomeCase some ? Maybe.Some(ifSome(some.Value)) : ifNone();

    public Maybe<TResult> Match<TResult>(Func<T, Maybe<TResult>> ifSome, Func<Maybe<TResult>> ifNone)
        => this is SomeCase some ? ifSome(some.Value) : ifNone();

    #region Private

    private Maybe() { }

    #endregion

    #region Nested types

    private sealed record NoneCase : Maybe<T>
    {
        #region Equality Members

        public bool Equals(NoneCase? other) => other == this;
        public override int GetHashCode() => 986349875;

        #endregion
    }

    private sealed record SomeCase(T Value) : Maybe<T>
    {
        #region Equality Members

        public bool Equals(SomeCase? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                   && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
            }
        }

        #endregion
    }

    #endregion
}
