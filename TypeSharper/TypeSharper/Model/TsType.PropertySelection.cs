namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public abstract record PropertySelection(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertyDuck(Info, Props)
    {
        #region Protected

        protected override Maybe<string> CsBody(TsModel model)
            => $$"""
                {
                {{CsCastsAndCtors(FromType, this, model).Indent()}}
                }
                """;

        #endregion

        #region Private

        private static string CsCastsAndCtors(TsTypeRef fromTypeRef, PropertyDuck selfType, TsModel model)
            => $$"""
                public static implicit operator {{selfType.Info.Name.Cs()}}({{fromTypeRef.Cs()}} from)
                    => new(from);
                public {{selfType.Info.Name.Cs()}}({{fromTypeRef.Cs()}} from)
                : this({{selfType.Props.Select(prop => prop.CsGetFrom("from")).JoinList()}}) { }
                {{model.Resolve(fromTypeRef)
                       .IfPropertySelection(propertySelection => CsCastsAndCtors(propertySelection.FromType, selfType, model))
                       .MapSomeOr("")}}
                """;

        #endregion
    }

    #endregion
}
