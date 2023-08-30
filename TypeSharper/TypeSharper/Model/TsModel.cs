using System.Collections.Generic;

namespace TypeSharper.Model;

public record TsModel(TsDict<TsTypeRef, TsType> TypeDict, TsDict<TsTypeRef, TsType> GeneratedTypeDict)
{
    public static TsModel New() => new(new TsDict<TsTypeRef, TsType>(), new TsDict<TsTypeRef, TsType>());

    public TsModel AddGeneratedType(TsType type)
        => this with { GeneratedTypeDict = GeneratedTypeDict.Add(type.Ref(), type) };

    public TsModel AddType(TsType type) => this with { TypeDict = TypeDict.Add(type.Ref(), type) };

    public TsType Resolve(TsTypeRef typeRef)
        => GeneratedTypeDict.TryGetValue(typeRef, out var normalType) ? normalType : TypeDict[typeRef];
}
