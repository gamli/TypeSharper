using System.Collections.Generic;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public class OmitGenerator : MemberSelectionTypeGenerator
{
    #region Protected

    protected override TsId AttributeId() => new("TypeSharperOmitAttribute");

    protected override TsModel DoGenerate(
        TsModel model,
        TsType fromType,
        TsDict<TsId, TsProp> fromTypePropertyLookup,
        TsList<TsId> selectedPropertyNames,
        TsType targetType,
        TsAttr attr)
    {
        var selectedMemberNamesSet = new HashSet<TsId>(selectedPropertyNames);
        return model.AddType(
            targetType.TypeKind is TsType.EKind.RecordClass or TsType.EKind.RecordStruct
                ? targetType
                  .NewPartial()
                  .SetPrimaryCtor(
                      new TsPrimaryCtor(
                          fromType
                              .Props
                              .Where(m => !selectedMemberNamesSet.Contains(m.Id))
                              .Select(
                                  propertyName =>
                                  {
                                      var prop = fromTypePropertyLookup[propertyName.Id];
                                      return new TsParam(prop.Type, prop.Id, false);
                                  })))
                : targetType
                  .NewPartial()
                  .AddProps(fromType.Props.Where(m => !selectedMemberNamesSet.Contains(m.Id))));
    }

    #endregion
}
