using System;
using System.Linq;
using TypeSharper.Model.Mod;

namespace TypeSharper.Model;

public record TsTypeRef : IComparable<TsTypeRef>
{
    public TsArrayMod ArrayMod { get; init; }
    public TsQualifiedName Name { get; init; }

    public static TsTypeRef WithNs(TsQualifiedName ns, TsQualifiedName name)
        => new(ns.Append(name), new TsArrayMod(false));

    public static TsTypeRef WithNsArray(TsQualifiedName ns, TsQualifiedName name)
        => new(ns.Append(name), new TsArrayMod(true));

    public static TsTypeRef WithoutNs(TsQualifiedName name) => new(name, new TsArrayMod(false));
    public static TsTypeRef WithoutNsArray(TsQualifiedName name) => new(name, new TsArrayMod(true));

    public TsTypeRef AddId(TsName name) => this with { Name = Name.Add(name) };

    public string Cs() => Name.Cs() + ArrayMod.Cs();

    public int CompareTo(TsTypeRef other)
        => string.Compare(Cs(), other.Cs(), StringComparison.InvariantCultureIgnoreCase);

    public override string ToString() => Cs();

    public TsTypeRef WithArrayMod(bool arrayMod) => WithArrayMod(new TsArrayMod(arrayMod));
    public TsTypeRef WithArrayMod(TsArrayMod arrayMod) => this with { ArrayMod = arrayMod };

    #region Private

    private TsTypeRef(TsQualifiedName name, TsArrayMod arrayMod)
    {
        if (name.Parts.Any(part => part.Cs() == "::global"))
        {
            throw new ArgumentOutOfRangeException(nameof(name), "::global should never be passed as ID");
        }

        if (name.Parts.Any(part => string.IsNullOrEmpty(part.Cs())))
        {
            throw new ArgumentOutOfRangeException(nameof(name), "namespace part should never be empty");
        }

        Name = name;
        ArrayMod = arrayMod;
    }

    #endregion
}
