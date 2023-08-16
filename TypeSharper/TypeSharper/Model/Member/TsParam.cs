using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsParam(TsTypeRef Type, TsId Id, bool IsParams)
{
    public string Cs() => $"{(IsParams ? "params " : "")}{Type.Cs()} {Id.Cs()}";
    public override string ToString() => Cs();
}
