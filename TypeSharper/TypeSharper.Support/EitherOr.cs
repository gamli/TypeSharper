using System;

namespace TypeSharper;

public abstract record EitherOr<TEither, TOr>
{
    public static EitherOr<TEither, TOr> Either(TEither value) => new EitherCase(value);

    public static implicit operator EitherOr<TEither, TOr>(TEither value) => Either(value);
    public static implicit operator EitherOr<TEither, TOr>(TOr value) => Or(value);
    public static EitherOr<TEither, TOr> Or(TOr value) => new OrCase(value);

    public void IfEither(Action<TEither> what)
    {
        if (this is EitherCase either)
        {
            what(either.Value);
        }
    }

    public Maybe<TResult> IfEither<TResult>(Func<TEither, TResult> what)
        => this is EitherCase either
            ? Maybe<TResult>.Some(what(either.Value))
            : Maybe<TResult>.NONE;

    public void IfOr(Action<TOr> what)
    {
        if (this is OrCase or)
        {
            what(or.Value);
        }
    }

    public Maybe<TResult> IfOr<TResult>(Func<TOr, TResult> what)
        => this is OrCase or
            ? Maybe<TResult>.Some(what(or.Value))
            : Maybe<TResult>.NONE;

    public void Match(Action<TEither> ifEither, Action<TOr> ifOr)
    {
        switch (this)
        {
            case EitherCase either:
                ifEither(either.Value);
                break;
            case OrCase or:
                ifOr(or.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public TResult Match<TResult>(Func<TEither, TResult> ifEither, Func<TOr, TResult> ifOr)
        => this switch
        {
            EitherCase either => ifEither(either.Value),
            OrCase or         => ifOr(or.Value),
            _                 => throw new ArgumentOutOfRangeException(),
        };

    #region Private

    private EitherOr() { }

    #endregion

    #region Nested types

    private sealed record EitherCase(TEither Value) : EitherOr<TEither, TOr>;

    private sealed record OrCase(TOr Value) : EitherOr<TEither, TOr>;

    #endregion
}
