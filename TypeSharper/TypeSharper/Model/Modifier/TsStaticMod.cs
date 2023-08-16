namespace TypeSharper.Model.Modifier;

public record TsStaticMod(bool IsSet)
{
    public string Cs() => IsSet ? "static" : "";
    public override string ToString() => Cs();
}
