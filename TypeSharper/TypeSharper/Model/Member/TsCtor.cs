using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Member;

public record TsCtor(
        TsList<TsParam> Params,
        TsMemberMods Mods,
        Maybe<string> CsBody)
    : TsMember(Mods)
{
    public string Cs(TsId typeId)
        => $"{Mods.Cs()} {typeId.Cs()}({CsParameters()}){CsBody.Match(csBody => $"\n{csBody}", () => ";")}";

    public override string ToString() => Cs(new TsId("__CTOR__"));

    #region Private

    private string CsParameters() => Params.Select(param => param.Cs()).JoinList();

    #endregion
}
