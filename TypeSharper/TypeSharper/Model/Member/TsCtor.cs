using System.Collections.Generic;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Member;

public record TsCtor(
        TsList<TsParam> Params,
        TsMemberMods Mods,
        Maybe<string> CsBody)
    : TsMember(Mods)
{
    public static TsCtor Public(Maybe<string> csBody, params TsParam[] parameters)
        => Public(csBody, (IEnumerable<TsParam>)parameters);

    public static TsCtor Public(Maybe<string> csBody, IEnumerable<TsParam> parameters)
        => new(
            TsList.Create(parameters),
            new TsMemberMods(
                ETsVisibility.Public,
                new TsAbstractMod(false),
                new TsStaticMod(false),
                ETsOperator.None),
            csBody);

    public string Cs(TsId typeId)
        => $"{Mods.Cs()} {typeId.Cs()}({CsParameters()}){CsBody.Match(csBody => $"\n{csBody}", () => ";")}";

    public override string ToString() => Cs(new TsId("__CTOR__"));

    #region Private

    private string CsParameters() => Params.Select(param => param.Cs()).JoinList();

    #endregion
}
