using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public record TsTypeRef(TsNs Ns, TsQualifiedId Id, bool IsArray = false)
{
    public string Cs() => Ns.Match(nsId => $"{nsId.Cs()}.{Id.Cs()}", () => Id.Cs()) + (IsArray ? "[]" : "");

    public override string ToString() => Cs();
}
