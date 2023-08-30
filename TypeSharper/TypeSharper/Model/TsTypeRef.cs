using System;
using System.Linq;
using TypeSharper.Model.Mod;

namespace TypeSharper.Model;

public record TsTypeRef : IComparable<TsTypeRef>
{
    public TsArrayMod ArrayMod { get; init; }
    public TsQualifiedName Name { get; init; }
    public TsList<TsTypeRef> TypeArguments { get; init; }

    public static TsTypeRef WithNs(TsQualifiedName ns, TsQualifiedName name)
        => WithNs(ns, name, TsList<TsTypeRef>.Empty);
    
    public static TsTypeRef WithNs(TsQualifiedName ns, TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(ns.Append(name), new TsArrayMod(false), typeArguments);

    public static TsTypeRef WithNsArray(TsQualifiedName ns, TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(ns.Append(name), new TsArrayMod(true), typeArguments);
    
    public static TsTypeRef WithNsArray(TsQualifiedName ns, TsQualifiedName name)
        => WithNsArray(ns, name, TsList<TsTypeRef>.Empty);

    public static TsTypeRef WithoutNs(TsQualifiedName name)
    => WithoutNs(name, TsList<TsTypeRef>.Empty);
    public static TsTypeRef WithoutNs(TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(name, new TsArrayMod(false), typeArguments);
    
    public static TsTypeRef WithoutNsArray(TsQualifiedName name)
        => WithoutNsArray(name, TsList<TsTypeRef>.Empty);
    public static TsTypeRef WithoutNsArray(TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(name, new TsArrayMod(true), typeArguments);

    public TsTypeRef AddId(TsName name) => this with { Name = Name.Add(name) };

    public string Cs()
        => Name.Cs()
           + (TypeArguments.Any() ? $"<{TypeArguments.Select(typeRef => typeRef.Cs()).JoinList()}>" : "")
           + ArrayMod.Cs();

    public int CompareTo(TsTypeRef other)
        => string.Compare(Cs(), other.Cs(), StringComparison.InvariantCultureIgnoreCase);

    public override string ToString() => Cs();

    public TsTypeRef WithArrayMod(bool arrayMod) => WithArrayMod(new TsArrayMod(arrayMod));
    public TsTypeRef WithArrayMod(TsArrayMod arrayMod) => this with { ArrayMod = arrayMod };

    #region Private

    private TsTypeRef(TsQualifiedName name, TsArrayMod arrayMod, TsList<TsTypeRef> typeArguments)
    {
        Name = name;
        ArrayMod = arrayMod;
        TypeArguments = typeArguments;
    }

    #endregion
}
