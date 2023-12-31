namespace TypeSharper.Model;

public abstract partial record TsType
{
    #region Nested types

    public sealed record Picked(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertySelection(Info, FromType, Props)
    {
        public override string ToString() => $"Picked:{base.ToString()}";
    }

    public record PickedAttr(
            TsTypeRef FromType,
            TsUniqueList<TsName> PropertyIdsToPick,
            TsList<TsPropMapping> PropMappings)
        : TsPropertySelectionAttr(FromType, PropertyIdsToPick, PropMappings);

    #endregion
}
