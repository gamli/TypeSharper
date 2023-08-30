namespace TypeSharper.Model;

public abstract partial record TsType
{
    public record OmittedAttr(TsTypeRef FromType, TsUniqueList<TsName> PropertyIdsToOmit) : TsAttr;

    public sealed record Omitted(TypeInfo Info, TsTypeRef FromType, TsUniqueList<TsProp> Props)
        : PropertySelection(Info, FromType, Props)
    {
        public override string ToString() => $"Omitted:{base.ToString()}";
    }
}
