using System;
using System.Collections.Generic;

namespace TypeSharper.Support;

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
    {
        if (this is NoneCase)
        {
            return new Maybe<TResult>.SomeCase(what());
        }

        return Maybe<TResult>.NONE;
    }

    public Maybe<TResult> IfNone<TResult>(Func<Maybe<TResult>> what)
    {
        if (this is NoneCase)
        {
            return what();
        }

        return Maybe<TResult>.NONE;
    }

    public void IfSome(Action<T> what)
    {
        if (this is SomeCase some)
        {
            what(some.Value);
        }
    }

    public Maybe<TResult> IfSome<TResult>(Func<T, TResult> what)
    {
        if (this is SomeCase some)
        {
            return new Maybe<TResult>.SomeCase(what(some.Value));
        }

        return Maybe<TResult>.NONE;
    }

    public Maybe<TResult> IfSome<TResult>(Func<T, Maybe<TResult>> what)
    {
        if (this is SomeCase some)
        {
            return what(some.Value);
        }

        return Maybe<TResult>.NONE;
    }

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
    {
        if (this is SomeCase some)
        {
            return ifSome(some.Value);
        }

        return ifNone();
    }

    public Maybe<TResult> Match<TResult>(Func<T, Maybe<TResult>> ifSome, Func<TResult> ifNone)
    {
        if (this is SomeCase some)
        {
            return ifSome(some.Value);
        }

        return Maybe<TResult>.Some(ifNone());
    }

    public Maybe<TResult> Match<TResult>(Func<T, TResult> ifSome, Func<Maybe<TResult>> ifNone)
    {
        if (this is SomeCase some)
        {
            return Maybe<TResult>.Some(ifSome(some.Value));
        }

        return ifNone();
    }

    public Maybe<TResult> Match<TResult>(Func<T, Maybe<TResult>> ifSome, Func<Maybe<TResult>> ifNone)
    {
        if (this is SomeCase some)
        {
            return ifSome(some.Value);
        }

        return ifNone();
    }

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
