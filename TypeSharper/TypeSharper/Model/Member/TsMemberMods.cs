using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Member;

public record TsMemberMods(ETsVisibility Visibility, TsAbstractMod Abstract, TsStaticMod Static, ETsOperator Operator)
{
    public string Cs() => new[] { Visibility.Cs(), Abstract.Cs(), Static.Cs(), Operator.Cs() }.JoinTokens();
    public override string ToString() => Cs();
}
