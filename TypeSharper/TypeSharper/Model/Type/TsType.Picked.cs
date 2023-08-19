using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    public static TsType CreatePicked(TypeInfo info, TsAttr typeSharperAttr, TsModel model)
        => new PickedImpl(info, typeSharperAttr, model);

    #region Nested types

    public interface Picked : PropertySelection { }

    private sealed record PickedImpl : PropertySelectionImpl, Picked
    {
        public PickedImpl(TypeInfo info, TsAttr typeSharperAttr, TsModel model)
            : base(
                info,
                typeSharperAttr,
                model,
                typeSharperAttr.FlattenedArgs().Select(arg => new TsId(arg))) { }

        public override string ToString() => $"{base.ToString()}:Picked";
    }

    #endregion
}
