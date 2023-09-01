using System;

namespace TypeSharper.Model;

public record TsProp(TsTypeRef Type, TsName Name) : IComparable<TsProp>
{
    public int CompareTo(TsProp other) => Name == other.Name ? Type.CompareTo(other.Type) : Name.CompareTo(other.Name);
    public string CsGetFrom(TsQualifiedName from) => $"{from.Cs()}.{Name.Cs()}";
    public string CsPrimaryCtor() => $"{Type.Cs()} {Name}";
    public string CsSet(string value) => $"{Name.Cs()} = {value}";
    public string CsSetFromGet(TsQualifiedName from) => CsSet(CsGetFrom(from));
}
