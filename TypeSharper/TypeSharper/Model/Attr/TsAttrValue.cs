using System;
using System.Collections.Generic;
using TypeSharper.Support;

namespace TypeSharper.Model.Attr;

public record TsAttrValue
{
    public static TsAttrValue Array(IEnumerable<string> values) => new ArrayCase(TsList.Create(values));
    public static TsAttrValue Primitive(string value) => new PrimitiveCase(value);
    public TsList<string> AssertArray() => Match(_ => throw new InvalidCastException(), array => array);
    public string AssertPrimitive() => Match(primitive => primitive, _ => throw new InvalidCastException());
    public string Cs() => Match(value => value, values => $"new string[] {{ {values.JoinList()}}}");

    public void Match(Action<string> primitive, Action<TsList<string>> array)
    {
        switch (this)
        {
            case PrimitiveCase singleCase:
                primitive(singleCase.Value);
                break;
            case ArrayCase multiCase:
                array(multiCase.Values);
                break;
        }
    }

    public TResult Match<TResult>(Func<string, TResult> primitive, Func<TsList<string>, TResult> array)
        => this switch
        {
            PrimitiveCase singleCase => primitive(singleCase.Value),
            ArrayCase multiCase      => array(multiCase.Values),
            _                        => throw new ArgumentOutOfRangeException(),
        };

    public override string ToString() => Cs();

    #region Private

    private TsAttrValue() { }

    #endregion

    #region Nested types

    private sealed record ArrayCase(TsList<string> Values) : TsAttrValue
    {
        #region Equality Members

        public bool Equals(ArrayCase? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && Values.Equals(other.Values);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Values.GetHashCode();
            }
        }

        #endregion
    }

    private sealed record PrimitiveCase(string Value) : TsAttrValue
    {
        #region Equality Members

        public bool Equals(PrimitiveCase? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && Value == other.Value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }

        #endregion
    }

    #endregion
}
