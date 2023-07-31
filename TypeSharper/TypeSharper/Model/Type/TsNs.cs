using System;
using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public record TsNs
{
    public static TsNs Global { get; } = new GlobalCase();

    public static TsNs Qualified(TsQualifiedId id) => new QualifiedCase(id);

    public string Cs() => Match(id => $"namespace {id.Cs()};", () => "");

    public T Match<T>(Func<TsQualifiedId, T> ifQualified, Func<T> ifGlobal)
        => this switch
        {
            QualifiedCase qualifiedCase => ifQualified(qualifiedCase.Id),
            GlobalCase                  => ifGlobal(),
            _                           => throw new ArgumentOutOfRangeException(),
        };

    public override string ToString() => Cs();

    #region Private

    private TsNs() { }

    #endregion

    #region Nested types

    private record GlobalCase : TsNs
    {
        #region Equality Members

        public virtual bool Equals(GlobalCase? other) => other == this;
        public override int GetHashCode() => 10239578;

        #endregion
    }

    private record QualifiedCase(TsQualifiedId Id) : TsNs
    {
        #region Equality Members

        public virtual bool Equals(QualifiedCase? other)
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
                   && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Id.GetHashCode();
            }
        }

        #endregion
    }

    #endregion
}
