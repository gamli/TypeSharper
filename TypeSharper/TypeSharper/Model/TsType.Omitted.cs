namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public sealed record Omitted(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertySelection(Info, FromType, Props)
    {
        public override string ToString() => $"Omitted:{base.ToString()}";
    }

    public record OmittedAttr(
            TsTypeRef FromType,
            TsUniqueList<TsName> PropertyIdsToOmit,
            TsList<TsPropMapping> PropMappings)
        : TsPropertySelectionAttr(FromType, PropertyIdsToOmit, PropMappings);

    #endregion
}
