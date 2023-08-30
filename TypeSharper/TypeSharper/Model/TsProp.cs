using System;
using System.Collections.Generic;

namespace TypeSharper.Model;

public record TsProp(TsTypeRef Type, TsName Name) : IComparable<TsProp>
{
    public string CsPrimaryCtor() => $"{Type.Cs()} {Name}";
    public string CsSetFromGet(TsQualifiedName from) => CsSet(CsGetFrom(from));
    public string CsGetFrom(TsQualifiedName from) => $"{from.Cs()}.{Name.Cs()}";
    public string CsSet(string value) => $"{Name.Cs()} = {value}";
    public int CompareTo(TsProp other) => Name == other.Name ? Type.CompareTo(other.Type) : Name.CompareTo(other.Name);
}
