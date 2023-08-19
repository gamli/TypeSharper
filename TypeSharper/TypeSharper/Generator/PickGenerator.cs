using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public class PickGenerator : MemberSelectionTypeGenerator
{
    #region Protected

    protected override TsId AttributeId() => new("TypeSharperPickAttribute");

    protected override TsModel DoGenerate(
        TsType targetType,
        TsAttr attr,
        TsModel model)
        => model.AddType(TsType.CreatePicked(targetType.Info, attr, model));

    #endregion
}
