namespace TypeSharper.Model;

public abstract partial record TsType
{
    public record PickedAttr(TsTypeRef FromType, TsUniqueList<TsName> PropertyIdsToPick) : TsAttr;

    public sealed record Picked(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertySelection(Info, FromType, Props)
    {
        public override string ToString() => $"Picked:{base.ToString()}";
    }
}
