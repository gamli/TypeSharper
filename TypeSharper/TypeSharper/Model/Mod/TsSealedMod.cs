namespace TypeSharper.Model.Mod;

public record TsSealedMod(bool IsSet)
{
    public string Cs() => IsSet ? "sealed" : "";
    public override string ToString() => Cs();
}
