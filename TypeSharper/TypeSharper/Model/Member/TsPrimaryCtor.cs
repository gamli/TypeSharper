using System.Collections.Generic;

namespace TypeSharper.Model.Member;

public record TsPrimaryCtor
{
    private TsPrimaryCtor(TsList<TsParam> parameters) => Params = parameters;

    public static Maybe<TsPrimaryCtor> Create(TsParam param) => Create(TsList.Create(param));
    public static Maybe<TsPrimaryCtor> Create(TsList<TsParam> parameters)
        => parameters.Count == 0
            ? Maybe<TsPrimaryCtor>.NONE
            : new TsPrimaryCtor(parameters);

    public string Cs() => $"({Params.Select(param => param.Cs()).JoinList()})";
    public TsList<TsParam> Params { get; }
}
