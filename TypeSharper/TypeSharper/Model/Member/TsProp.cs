using System;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsProp(
        TsTypeRef Type,
        TsId Id,
        TsMemberMods Mods,
        TsProp.BodyImpl Body)
    : TsMember(Mods)
{
    public static TsProp RecordPrimaryCtorProp(TsTypeRef type, TsId id)
        => new (
            type,
            id,
            new TsMemberMods(
                ETsVisibility.Public,
                new TsAbstractMod(false),
                new TsStaticMod(false),
                ETsOperator.None),
            BodyImpl.Accessors(new TsList<TsPropAccessor> { TsPropAccessor.PublicGet() }));

    public string Cs() => $"{Mods.Cs()} {Type.Cs()} {Id.Cs()} {Body.Cs()}";
    public string CsGetFrom(params string[] ids) => CsGetFrom(new TsQualifiedId(ids));
    public string CsGetFrom(params TsId[] ids) => CsGetFrom(new TsQualifiedId(ids));
    public string CsGetFrom(TsQualifiedId fromObjectId) => $"{fromObjectId.Cs()}.{Id.Cs()}";
    public string CsSet(string value) => $"{Id.Cs()} = {value}";

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
