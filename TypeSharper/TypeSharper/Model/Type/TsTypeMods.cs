using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public record TsTypeMods(
    ETsVisibility Visibility,
    TsAbstractMod Abstract,
    TsStaticMod Static,
    TsSealedMod Sealed,
    TsPartialMod Partial,
    TsTargetTypeMod TargetType)
{
    public string Cs() => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs(), Sealed.Cs(), Partial.Cs() }.JoinTokens();
    public override string ToString() => Cs();
}
