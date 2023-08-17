using System;
using System.Linq;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsMethod(
        TsId Id,
        TsTypeRef ReturnType,
        TsList<TsTypeRef> TypeParams,
        TsList<TsParam> Params,
        TsMemberMods Mods,
        Maybe<string> CsBody)
    : TsMember(Mods)
{
    public static TsMethod CastOperator(
        TsTypeRef fromType,
        TsTypeRef toType,
        Func<TsParam, string> csBody,
        ETsOperator op)
        => new(
            new TsId(""),
            toType,
            new TsList<TsTypeRef>(),
            TsList.Create(fromType.ToParam("fromValue")),
            new TsMemberMods(ETsVisibility.Public, new TsAbstractMod(false), new TsStaticMod(true), op),
            csBody(fromType.ToParam("fromValue")));

    public static TsMethod ExplicitCastOperator(TsTypeRef fromType, TsTypeRef toType, Func<TsParam, string> csBody)
        => CastOperator(fromType, toType, csBody, ETsOperator.Explicit);

    public static TsMethod ImplicitCastOperator(TsTypeRef fromType, TsTypeRef toType, Func<TsParam, string> csBody)
        => CastOperator(fromType, toType, csBody, ETsOperator.Implicit);

    public string Cs() => $"{CsSignature()}{CsBody.Match(csBody => $"\n{csBody}", () => ";")}";

    public override string ToString() => Cs();

    #region Private

    private string CsName() => Mods.Operator is ETsOperator.None ? Id.Cs() : "";
    private string CsParams() => Params.Select(p => p.Cs()).JoinList();

    private string CsSignature()
        => $"{Mods.Cs()} {ReturnType.Cs()} {CsName().MarginRight()}{CsTypeParams()}({CsParams()})";

    private string CsTypeParams()
        => TypeParams.Any() ? $"<{TypeParams.Select(typeRef => typeRef.Cs()).JoinList()}>" : "";

    #endregion
}
