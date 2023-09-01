using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeSharper.Model.Mod;

namespace TypeSharper.Model;

public record TsTypeRef : IComparable<TsTypeRef>
{
    public TsArrayMod ArrayMod { get; init; }
    public TsQualifiedName Name { get; init; }
    public TsList<TsTypeRef> TypeArguments { get; init; }

    public static TsTypeRef FromTypeSyntax(TypeSyntax typeSyntax)
        => typeSyntax switch
        {
            GenericNameSyntax genericType => new TsTypeRef(
                new TsQualifiedName(genericType.Identifier.ToString().Split(".")),
                new TsArrayMod(false),
                new TsList<TsTypeRef>(genericType.TypeArgumentList.Arguments.Select(FromTypeSyntax))),
            ArrayTypeSyntax arrayType => new TsTypeRef(
                new TsQualifiedName(arrayType.ElementType.ToString().Split(".")),
                new TsArrayMod(true),
                new TsList<TsTypeRef>()),
            _ => new TsTypeRef(
                new TsQualifiedName(typeSyntax.ToString().Split(".")),
                new TsArrayMod(false),
                new TsList<TsTypeRef>()),
        };

    public static TsTypeRef Parse(string csTypeRef) => FromTypeSyntax(SyntaxFactory.ParseTypeName(csTypeRef));

    public static TsTypeRef WithNs(TsQualifiedName ns, TsQualifiedName name)
        => WithNs(ns, name, TsList<TsTypeRef>.Empty);

    public static TsTypeRef WithNs(TsQualifiedName ns, TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(ns.Append(name), new TsArrayMod(false), typeArguments);

    public static TsTypeRef WithNsArray(TsQualifiedName ns, TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(ns.Append(name), new TsArrayMod(true), typeArguments);

    public static TsTypeRef WithNsArray(TsQualifiedName ns, TsQualifiedName name)
        => WithNsArray(ns, name, TsList<TsTypeRef>.Empty);

    public static TsTypeRef WithoutNs(TsQualifiedName name) => WithoutNs(name, TsList<TsTypeRef>.Empty);

    public static TsTypeRef WithoutNs(TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(name, new TsArrayMod(false), typeArguments);

    public static TsTypeRef WithoutNsArray(TsQualifiedName name) => WithoutNsArray(name, TsList<TsTypeRef>.Empty);

    public static TsTypeRef WithoutNsArray(TsQualifiedName name, TsList<TsTypeRef> typeArguments)
        => new(name, new TsArrayMod(true), typeArguments);

    public TsTypeRef AddId(TsName name) => this with { Name = Name.Add(name) };

    public int CompareTo(TsTypeRef other)
        => string.Compare(Cs(), other.Cs(), StringComparison.InvariantCultureIgnoreCase);

    public string Cs()
        => Name.Cs()
           + (TypeArguments.Any() ? $"<{TypeArguments.Select(typeRef => typeRef.Cs()).JoinList()}>" : "")
           + ArrayMod.Cs();

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
