namespace TypeSharper.Model.Member;

public record TsPrimaryCtor(TsList<TsParam> Params)
{
    public string Cs() => $"({Params.Select(param => param.Cs()).JoinList()})";
}
