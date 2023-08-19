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
    public static TsTypeMods PrivateSealed = new(
        ETsVisibility.Private,
        new TsAbstractMod(false),
        new TsStaticMod(false),
        new TsSealedMod(true),
        new TsPartialMod(false),
        new TsTargetTypeMod(false));
    
    public static TsTypeMods PublicSealed = new(
        ETsVisibility.Public,
        new TsAbstractMod(false),
        new TsStaticMod(false),
        new TsSealedMod(true),
        new TsPartialMod(false),
        new TsTargetTypeMod(false));

    public static TsTypeMods PublicPartial = new(
        ETsVisibility.Public,
        new TsAbstractMod(false),
        new TsStaticMod(false),
        new TsSealedMod(false),
        new TsPartialMod(true),
        new TsTargetTypeMod(false));

    public static TsTypeMods PublicPartialAbstract = new(
        ETsVisibility.Public,
        new TsAbstractMod(true),
        new TsStaticMod(false),
        new TsSealedMod(false),
        new TsPartialMod(true),
        new TsTargetTypeMod(false));

    public string Cs() => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs(), Sealed.Cs(), Partial.Cs() }.JoinTokens();
    public override string ToString() => Cs();
}
