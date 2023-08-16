using System.Linq;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;

namespace TypeSharper.Model.Attr.Def;

public record TsAttrOverloadDef(
    TsList<TsParam> CtorParameters,
    TsList<TsParam> NamedParameters,
    TsList<TsId> TypeParameters)
{
    public string CsConstructor(TsId attrTypeId)
        => $$"""
            public {{attrTypeId.Cs()}}({{CtorParameters.Select(p => $"{CsParamsModifier(p).MarginRight()}{p.Type.Cs()} {p.Id.Cs()}").JoinList()}})
            {
            {{CtorParameters.Select(p => $"{p.Id.Cs().Capitalize()} = {p.Id};").Indent()}}
            }
            """;

    public string CsProperties()
        => string.Join(
            "\n",
            CtorParameters
                .Concat(NamedParameters)
                .Select(parameter => $"public {parameter.Type.Cs()} {parameter.Id.Cs().Capitalize()} {{ get; set; }}"));

    public string CsTypeParameters()
        => TypeParameters.Count == 0
            ? ""
            : $"<{string.Join(", ", TypeParameters.Select(typeParameter => typeParameter.Cs()))}>";

    #region Private

    private string CsParamsModifier(TsParam param) => param.IsParams ? "params" : "";

    #endregion

    #region Equality Members

    public virtual bool Equals(TsAttrOverloadDef? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return CtorParameters.Equals(other.CtorParameters)
               && NamedParameters.Equals(other.NamedParameters)
               && TypeParameters.Equals(other.TypeParameters);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = CtorParameters.GetHashCode();
            hashCode = (hashCode * 397) ^ NamedParameters.GetHashCode();
            hashCode = (hashCode * 397) ^ TypeParameters.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
