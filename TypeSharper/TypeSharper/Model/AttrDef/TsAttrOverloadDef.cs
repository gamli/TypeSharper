using System.Linq;

namespace TypeSharper.Model.AttrDef;

public record TsAttrOverloadDef(
    TsList<TsAttrOverloadDef.Param> CtorParameters,
    TsList<TsAttrOverloadDef.Param> NamedParameters,
    TsList<TsName> TypeParameters)
{
    public string CsConstructor(TsName attrTypeName)
        => $$"""
            public {{attrTypeName.Cs()}}({{CtorParameters.Select(p => $"{CsParamsModifier(p).MarginRight()}{p.Type.Cs()} {p.Name.Cs()}").JoinList()}})
            {
            {{CtorParameters.Select(p => $"{p.Name.Cs().Capitalize()} = {p.Name};").Indent()}}
            }
            """;

    public string CsProperties()
        => string.Join(
            "\n",
            CtorParameters
                .Concat(NamedParameters)
                .Select(
                    parameter => $"public {parameter.Type.Cs()} {parameter.Name.Cs().Capitalize()} {{ get; set; }}"));

    public string CsTypeParameters()
        => TypeParameters.Count == 0
            ? ""
            : $"<{string.Join(", ", TypeParameters.Select(typeParameter => typeParameter.Cs()))}>";

    #region Private

    private string CsParamsModifier(Param param) => param.IsParams ? "params" : "";

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

    #region Nested types

    public record Param(TsTypeRef Type, TsName Name, bool IsParams = false);

    #endregion
}
