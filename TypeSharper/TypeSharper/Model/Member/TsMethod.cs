using TypeSharper.Model.Identifier;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsMethod(
        TsId Id,
        TsTypeRef ReturnType,
        TsList<TsParam> Params,
        TsMemberMods Mods,
        Maybe<string> CsBody)
    : TsMember(Mods)
{
    public string Cs()
        => Mods.Operator is ETsOperator.None
            ? $"{Mods.Cs()} {ReturnType.Cs()} {Id.Cs()}({CsParameters()}){CsBody.Match(csBody => $"\n{csBody}", () => ";")}"
            : $"{Mods.Cs()} {ReturnType.Cs()}({CsParameters()}){CsBody.Match(csBody => $"\n{csBody}", () => ";")}";

    public override string ToString() => Cs();

    #region Private

    private string CsParameters() => Params.Select(p => p.Cs()).JoinList();

    #endregion
}
