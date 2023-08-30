using System;

namespace TypeSharper;

public abstract record EitherOr<TEither, TOr>
{
    public static EitherOr<TEither, TOr> Either(TEither value) => new EitherCase(value);

    public static implicit operator EitherOr<TEither, TOr>(TEither value) => Either(value);
    public static implicit operator EitherOr<TEither, TOr>(TOr value) => Or(value);
    public static EitherOr<TEither, TOr> Or(TOr value) => new OrCase(value);

    public EitherOr<TEither, TOr> IfEither(Func<TEither, EitherOr<TEither, TOr>> what)
        => Map<EitherOr<TEither, TOr>>(what, or => or);
    
    public EitherOr<TResult, TOr> IfEither<TResult>(Func<TEither, TResult> what)
        => Map<EitherOr<TResult, TOr>>(either => what(either), or => or);

    public void IfOr(Action<TOr> what)
    {
        if (this is OrCase or)
        {
            what(or.Value);
        }
    }

    public EitherOr<TEither, TResult> IfOr<TResult>(Func<TOr, TResult> what)
        => Map<EitherOr<TEither, TResult>>(either => either, or => what(or));

    public void Map(Action<TEither> ifEither, Action<TOr> ifOr)
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

    public TResult Map<TResult>(Func<TEither, TResult> ifEither, Func<TOr, TResult> ifOr)
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
