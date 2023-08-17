namespace TypeSharper.Model.Modifier;

public record TsArrayMod(bool IsSet)
{
    public string Cs() => IsSet ? "[]" : "";
}
