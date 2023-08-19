using System.Collections.Generic;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    public static TsType CreateOmitted(TypeInfo info, TsAttr typeSharperAttr, TsModel model)
        => new OmittedImpl(info, typeSharperAttr, model);

    #region Nested types

    public interface Omitted : Duck { }

    private sealed record OmittedImpl : PropertySelectionImpl, Omitted
    {
        public OmittedImpl(TypeInfo info, TsAttr typeSharperAttr, TsModel model)
            : base(
                info,
                typeSharperAttr,
                model,
                OmittedPropIds(typeSharperAttr, model)) { }

        public override string ToString() => $"{base.ToString()}:Omitted";

        #region Private

        private static TsList<TsId> OmittedPropIds(TsAttr typeSharperAttr, TsModel model)
        {
            var omittedPropertyIds = new HashSet<TsId>(typeSharperAttr.FlattenedArgs().Select(arg => new TsId(arg)));
            return model
                   .Resolve(ExtractFromTypeRef(typeSharperAttr))
                   .Props
                   .Where(prop => !omittedPropertyIds.Contains(prop.Id))
                   .Select(prop => prop.Id);
        }

        #endregion
    }

    #endregion
}
