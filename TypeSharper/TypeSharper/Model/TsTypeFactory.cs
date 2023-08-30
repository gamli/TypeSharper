using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeSharper.Model;

public static class TsTypeFactory
{
    public static TsType Create(TsType.TypeInfo info, TsAttr attr, TsModel model)
        => attr switch
        {
            TsType.IntersectionAttr intersectionAttr => Create(info, intersectionAttr, model),
            TsType.OmittedAttr omittedAttr           => Create(info, omittedAttr, model),
            TsType.PickedAttr pickedAttr             => Create(info, pickedAttr, model),
            TsType.TaggedUnionAttr taggedUnionAttr   => Create(info, taggedUnionAttr, model),
            _                                        => throw new ArgumentOutOfRangeException(nameof(attr)),
        };

    public static TsType CreateNative(TsType.TypeInfo typeInfo, IEnumerable<TsProp> props)
        => new TsType.Native(typeInfo, TsUniqueList.Create(props));

    #region Private

    private static TsType Create(
        TsType.TypeInfo info,
        TsType.PickedAttr pickedAttr,
        TsModel model)
        => new TsType.Picked(
            info,
            pickedAttr.FromType,
            TsUniqueList.CreateRange(
                FromTypeProperties(pickedAttr.FromType, model)
                    .Where(prop => pickedAttr.PropertyIdsToPick.Contains(prop.Name))));

    private static TsType Create(
        TsType.TypeInfo info,
        TsType.OmittedAttr omitAttr,
        TsModel model)
        => new TsType.Omitted(
            info,
            omitAttr.FromType,
            TsUniqueList.CreateRange(
                FromTypeProperties(omitAttr.FromType, model)
                    .Where(prop => !omitAttr.PropertyIdsToOmit.Contains(prop.Name))));

    private static TsType Create(
        TsType.TypeInfo info,
        TsType.IntersectionAttr intersectionAttr,
        TsModel model)
    {
        var propss =
            intersectionAttr
                .TypesToIntersect
                .Select(model.Resolve)
                .Select(
                    type => type.MapPropertyDuck(
                        propertyDuck => propertyDuck.Props,
                        _ => TsUniqueList.Create<TsProp>(),
                        native => native.Props))
                .ToList();
        return new TsType.Intersection(
            info,
            intersectionAttr.TypesToIntersect,
            TsUniqueList.CreateRange(
                propss
                    .First()
                    .Where(candidateProp => propss.All(props => props.Any(prop => prop.Name == candidateProp.Name)))));
    }

    private static TsType Create(
        TsType.TypeInfo info,
        TsType.TaggedUnionAttr taggedUnionAttr,
        TsModel model)
        => new TsType.TaggedUnion(
            info,
            TsUniqueList.Create(
                taggedUnionAttr
                    .CaseNames
                    .Zip(
                        taggedUnionAttr
                            .CaseTypes
                            .Select(Maybe<TsTypeRef>.Some)
                            .Concat(Maybe<TsTypeRef>.NONE.Repeat()),
                        (name, valueType) => new TsType.TaggedUnion.Case(name, valueType))));


    private static TsUniqueList<TsProp> FromTypeProperties(TsTypeRef fromType, TsModel model)
        => model
           .Resolve(fromType)
           .MapPropertyDuck(
               propertyDuck => propertyDuck.Props,
               _ => TsUniqueList.Create<TsProp>(),
               native => native.Props);

    #endregion
}
