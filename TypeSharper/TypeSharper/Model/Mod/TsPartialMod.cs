namespace TypeSharper.Model.Mod;

public record TsPartialMod(bool IsSet)
{
    public string Cs() => IsSet ? "partial" : "";
    public override string ToString() => Cs();
}
