namespace TypeSharper.Model.Mod;

public record TsStaticMod(bool IsSet)
{
    public string Cs() => IsSet ? "static" : "";
    public override string ToString() => Cs();
}
