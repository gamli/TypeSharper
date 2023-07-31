using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Type;

namespace TypeSharper.Model;

public record TsModel(TsDict<TsTypeRef, TsType> TypeLookup)
{
    public IEnumerable<TsType> Types => TypeLookup.Values;

    public static TsModel New() => new(new TsDict<TsTypeRef, TsType>());

    public TsModel AddType(TsType type)
    {
        var typeRef = type.Ref();
        return TypeLookup.TryGetValue(typeRef, out var existingType)
            ? new TsModel(
                TypeLookup.Set(
                    typeRef,
                    existingType.AddMembers(type.Ctors, type.Props, type.Methods, type.Attrs)))
            : new TsModel(TypeLookup.Add(typeRef, type));
    }

    public TsModel Diff(TsModel otherModel)
        => otherModel
           .Types
           .Where(otherType => !otherType.IsTopLevel() || HasDiff(otherType, otherModel))
           .Select(
               otherType =>
               {
                   var otherTypeRef = otherType.Ref();
                   return TypeLookup.ContainsKey(otherTypeRef)
                       ? TypeLookup[otherTypeRef].Diff(otherType)
                       : otherType;
               })
           .Aggregate(New(), (accModel, type) => accModel.AddType(type));

    public IEnumerable<TsType> NestedTypes(TsType type)
        => Types
           .Where(
               maybeNestedType => maybeNestedType.ContainingType.Match(
                   nestedType => nestedType == type.Ref(),
                   () => false))
           .OrderBy(nestedType => nestedType.Id.Cs());

    public TsType Resolve(TsTypeRef typeRef) => TypeLookup[typeRef];

    #region Private

    private bool HasDiff(TsType otherType, TsModel otherModel)
    {
        var otherTypeRef = otherType.Ref();
        return !TypeLookup.ContainsKey(otherTypeRef)
               || TypeLookup[otherTypeRef] != otherType
               || otherModel
                  .NestedTypes(otherType)
                  .Any(otherNestedType => HasDiff(otherModel.TypeLookup[otherNestedType.Ref()], otherModel));
    }

    #endregion
}
