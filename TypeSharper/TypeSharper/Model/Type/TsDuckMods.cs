using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public record TsDuckMods(
    ETsVisibility Visibility,
    TsAbstractMod Abstract,
    TsStaticMod Static,
    TsSealedMod Sealed)
{
    public string Cs()
        => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs(), Sealed.Cs(), new TsPartialMod(true).Cs() }.JoinTokens();

    public override string ToString() => Cs();
}
