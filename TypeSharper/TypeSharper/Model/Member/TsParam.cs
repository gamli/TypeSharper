using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Model.Member;

public record TsParam(TsTypeRef Type, TsId Id, bool IsParams = false)
{
    public string Cs() => $"{(IsParams ? "params " : "")}{Type.Cs()} {Id.Cs()}";
    public string CsRef() => Id.Cs();
    public TsProp ToPrimaryCtorProp() => TsProp.RecordPrimaryCtorProp(Type, Id);
    public override string ToString() => Cs();
}
