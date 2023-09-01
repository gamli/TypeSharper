using System;
using System.Linq;
using TypeSharper.Model.Mod;

namespace TypeSharper.Model;

public record TsTypeMods(
    ETsVisibility Visibility,
    TsAbstractMod Abstract,
    TsStaticMod Static,
    TsSealedMod Sealed,
    TsPartialMod Partial) : IComparable<TsTypeMods>
{
    public static TsTypeMods PrivateSealed = new(
        ETsVisibility.Private,
        new TsAbstractMod(false),
        new TsStaticMod(false),
        new TsSealedMod(true),
        new TsPartialMod(false));

    public static TsTypeMods PublicPartial = new(
        ETsVisibility.Public,
        new TsAbstractMod(false),
        new TsStaticMod(false),
        new TsSealedMod(false),
        new TsPartialMod(true));

    public static TsTypeMods PublicPartialAbstract = new(
        ETsVisibility.Public,
        new TsAbstractMod(true),
        new TsStaticMod(false),
        new TsSealedMod(false),
        new TsPartialMod(true));

    public static TsTypeMods PublicSealed = new(
        ETsVisibility.Public,
        new TsAbstractMod(false),
        new TsStaticMod(false),
        new TsSealedMod(true),
        new TsPartialMod(false));

    public int CompareTo(TsTypeMods other)
    {
        var (self, oth) =
            new (int self, int oth)[]
                {
                    ((int)Visibility, (int)other.Visibility),
                    (Abstract.IsSet ? 1 : 0, other.Abstract.IsSet ? 1 : 0),
                    (Static.IsSet ? 1 : 0, other.Static.IsSet ? 1 : 0),
                }
                .FirstOrDefault(t => t.self != t.oth);
        return self.CompareTo(oth);
    }

    public string Cs() => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs(), Sealed.Cs(), Partial.Cs() }.JoinTokens();

    public override string ToString() => Cs();
}
