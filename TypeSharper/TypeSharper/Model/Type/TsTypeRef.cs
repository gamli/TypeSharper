using System;
using System.Linq;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public record TsTypeRef
{
    public TsArrayMod ArrayMod { get; init; }
    public TsQualifiedId Id { get; init; }
    public static TsTypeRef WithNs(TsQualifiedId ns, TsQualifiedId id) => new(ns.Append(id), new TsArrayMod(false));
    public static TsTypeRef WithNsArray(TsQualifiedId ns, TsQualifiedId id) => new(ns.Append(id), new TsArrayMod(true));

    public static TsTypeRef WithoutNs(TsQualifiedId id) => new(id, new TsArrayMod(false));
    public static TsTypeRef WithoutNsArray(TsQualifiedId id) => new(id, new TsArrayMod(true));

    public TsTypeRef AddId(TsId id) => this with { Id = Id.Add(id) };

    public string Cs() => Id.Cs() + ArrayMod.Cs();

    public override string ToString() => Cs();

    public TsTypeRef WithArrayMod(bool arrayMod) => WithArrayMod(new TsArrayMod(arrayMod));
    public TsTypeRef WithArrayMod(TsArrayMod arrayMod) => this with { ArrayMod = arrayMod };

    #region Private

    private TsTypeRef(TsQualifiedId id, TsArrayMod arrayMod)
    {
        if (id.Parts.Any(part => part.Cs() == "::global"))
        {
            throw new ArgumentOutOfRangeException(nameof(id), "::global should never be passed as ID");
        }

        if (id.Parts.Any(part => string.IsNullOrEmpty(part.Cs())))
        {
            throw new ArgumentOutOfRangeException(nameof(id), "namespace part should never be empty");
        }

        Id = id;
        ArrayMod = arrayMod;
    }

    #endregion
}
