using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsPrimaryCtor
{
    public TsList<TsId> PropAndParamIds { get; }

    public static Maybe<TsPrimaryCtor> Create(TsId propAndParamId) => Create(TsList.Create(propAndParamId));

    public static Maybe<TsPrimaryCtor> Create(TsList<TsId> propAndParamIds)
        => propAndParamIds.Count == 0
            ? Maybe<TsPrimaryCtor>.NONE
            : new TsPrimaryCtor(propAndParamIds);

    public string Cs(TsType containingType)
    {
        var ps = PropAndParamIds.Select(
            propAndParamId => new TsParam(containingType.Prop(propAndParamId).Type, propAndParamId));
        return $"({ps.Select(param => param.Cs()).JoinList()})";
    }

    #region Private

    private TsPrimaryCtor(TsList<TsId> propAndParamIds) => PropAndParamIds = propAndParamIds;

    #endregion
}
