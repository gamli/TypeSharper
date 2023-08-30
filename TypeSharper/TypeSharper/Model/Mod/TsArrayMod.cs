namespace TypeSharper.Model.Mod;

public record TsArrayMod(bool IsSet)
{
    public string Cs() => IsSet ? "[]" : "";
}
