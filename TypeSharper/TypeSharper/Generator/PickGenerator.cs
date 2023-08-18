using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public class PickGenerator : MemberSelectionTypeGenerator
{
    #region Protected

    protected override TsId AttributeId() => new("TypeSharperPickAttribute");

    protected override TsType DoGenerate(
        TsModel model,
        TsType fromType,
        TsDict<TsId, TsProp> fromTypePropertyLookup,
        TsList<TsId> selectedPropertyNames,
        TsType targetType,
        TsAttr attr)
        => targetType.SupportsPrimaryCtor
            ? targetType
              .NewPartial()
              .SetPrimaryCtor(
                  TsPrimaryCtor.Create(
                      selectedPropertyNames.Select(
                          propertyName =>
                          {
                              var prop = fromTypePropertyLookup[propertyName];
                              return new TsParam(prop.Type, prop.Id, false);
                          })))
            : targetType
              .NewPartial()
              .AddProps(selectedPropertyNames.Select(propertyName => fromTypePropertyLookup[propertyName]));

    #endregion
}
