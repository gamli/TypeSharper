using System;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;
using TypeSharper.Support;

namespace TypeSharper.Model.Member;

public record TsProp(
        TsTypeRef Type,
        TsId Id,
        TsMemberMods Mods,
        TsProp.BodyImpl Body)
    : TsMember(Mods)
{
    public string Cs() => $"{Mods.Cs()} {Type.Cs()} {Id.Cs()} {Body.Cs()}";
    public string CsAssign(string csExpression) => $"{Id.Cs()} = {csExpression};";

    #region Equality Members

    public virtual bool Equals(TsProp? other)
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
               && Type.Equals(other.Type)
               && Id.Equals(other.Id)
               && Body.Equals(other.Body);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();
            hashCode = (hashCode * 397) ^ Id.GetHashCode();
            hashCode = (hashCode * 397) ^ Body.GetHashCode();
            return hashCode;
        }
    }

    #endregion

    #region Nested types

    public record BodyImpl
    {
        public static BodyImpl Accessors(TsList<TsPropAccessor> accessors) => new AccessorsCase(accessors);
        public static BodyImpl Expression(string cs) => new ExpressionCase(cs);

        public string Cs()
            => Match(
                accessors => $"{{ {accessors.Select(accessor => accessor.Cs()).JoinTokens().MarginRight()}}}",
                csExpression => $"=> {csExpression};");

        public T Match<T>(Func<TsList<TsPropAccessor>, T> ifAccessors, Func<string, T> ifExpression)
            => this switch
            {
                AccessorsCase accessorsCase   => ifAccessors(accessorsCase.Value),
                ExpressionCase expressionCase => ifExpression(expressionCase.Value),
                _                             => throw new ArgumentOutOfRangeException(),
            };

        public override string ToString() => Cs();

        #region Private

        private BodyImpl() { }

        #endregion

        #region Nested types

        private record AccessorsCase(TsList<TsPropAccessor> Value) : BodyImpl
        {
            #region Equality Members

            public virtual bool Equals(AccessorsCase? other)
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
                       && Value.Equals(other.Value);
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

        private record ExpressionCase(string Value) : BodyImpl
        {
            #region Equality Members

            public virtual bool Equals(ExpressionCase? other)
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
                       && Value == other.Value;
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

    #endregion
}
