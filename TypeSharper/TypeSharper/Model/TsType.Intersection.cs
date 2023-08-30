using System.Linq;

namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public sealed record Intersection(
            TypeInfo Info,
            TsUniqueList<TsTypeRef> TypesToIntersect,
            TsUniqueList<TsProp> Props)
        : PropertyDuck(Info, Props)
    {
        public override string ToString() => $"Intersection:{base.ToString()}";

        #region Protected

        protected override Maybe<string> CsBody(TsModel model)
            => $$"""
                {
                {{TypesToIntersect.Select(CsConstituentTypeCtor).JoinLines().Indent()}}
                {{TypesToIntersect.Select(CsConstituentTypeCastOperator).JoinLines().Indent()}}
                }
                """;

        #endregion

        #region Private

        private string CsConstituentTypeCtor(TsTypeRef type)
        {
            var csTypeName = Info.Name.Cs();
            var csFromTypeName = type.Cs();
            return Props.Any()
                ? $$"""
                public {{csTypeName}}({{csFromTypeName}} from)
                : this({{Props.Select(prop => prop.CsGetFrom("from")).JoinList()}}) { }
                """
                : $$"""
                public {{csTypeName}}({{csFromTypeName}} _) { }
                """;
        }

        private string CsConstituentTypeCastOperator(TsTypeRef type)
            => $$"""
                public static implicit operator {{Info.Name.Cs()}}({{type.Cs()}} from)
                    => new(from);
                """;

        #endregion
    }

    public record IntersectionAttr(TsUniqueList<TsTypeRef> TypesToIntersect) : TsAttr;

    #endregion
}
