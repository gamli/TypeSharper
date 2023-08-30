using System;
using System.Linq;

namespace TypeSharper.Model;

public record TsNs(TsQualifiedName FullyQualifiedName) : IComparable<TsNs>
{
    public static implicit operator TsNs(string csNs) => new(csNs);
    public static implicit operator TsNs(TsQualifiedName qualifiedName) => new(qualifiedName);

    public string CsFileScoped()
        => !FullyQualifiedName.Parts.Any() || FullyQualifiedName.Parts.First() == "::global"
            ? ""
            : $"namespace {FullyQualifiedName.Cs()};";

    public int CompareTo(TsNs other) => FullyQualifiedName.CompareTo(other.FullyQualifiedName);
    public override string ToString() => CsFileScoped();
}
