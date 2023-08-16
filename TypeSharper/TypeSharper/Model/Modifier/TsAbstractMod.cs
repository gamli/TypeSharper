namespace TypeSharper.Model.Modifier;

public record TsAbstractMod(bool IsSet)
{
    public string Cs() => IsSet ? "abstract" : "";
    public override string ToString() => Cs();
}
