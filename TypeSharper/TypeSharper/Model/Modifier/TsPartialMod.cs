namespace TypeSharper.Model.Modifier;

public record TsPartialMod(bool IsSet)
{
    public string Cs() => IsSet ? "partial" : "";
    public override string ToString() => Cs();
}
