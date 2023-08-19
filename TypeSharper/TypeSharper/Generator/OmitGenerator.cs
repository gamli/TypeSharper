using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public class OmitGenerator : MemberSelectionTypeGenerator
{
    #region Protected

    protected override TsId AttributeId() => new("TypeSharperOmitAttribute");

    protected override TsModel DoGenerate(
        TsType targetType,
        TsAttr attr,
        TsModel model)
        => model.AddType(TsType.CreateOmitted(targetType.Info, attr, model));

    #endregion
}
