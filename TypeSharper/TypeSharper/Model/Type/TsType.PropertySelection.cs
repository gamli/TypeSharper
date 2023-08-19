using System.Linq;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    #region Nested types

    public interface PropertySelection : Duck
    {
        TsTypeRef FromTypeRef { get; }
        TsList<TsId> SelectedPropIds { get; }
        TsType FromType(TsModel model);
        TsList<TsProp> SelectedProps(TsModel model);
    }

    private record PropertySelectionImpl : DuckImpl, PropertySelection
    {
        public TsTypeRef FromTypeRef => ExtractFromTypeRef(TypeSharperAttr);
        public TsList<TsId> SelectedPropIds { get; }

        public TsType FromType(TsModel model) => model.Resolve(TypeSharperAttr.TypeArgs.Single());

        public TsList<TsProp> SelectedProps(TsModel model)
            => ResolveProperties(SelectedPropIds, TypeSharperAttr, model);

        #region Protected

        protected PropertySelectionImpl(
            TypeInfo info,
            TsAttr typeSharperAttr,
            TsModel model,
            TsList<TsId> selectedPropIds)
            : base(
                info,
                typeSharperAttr,
                TsList.Create(typeSharperAttr.TypeArgs.Single()),
                TsPrimaryCtor.Create(selectedPropIds),
                TsList.Create(FromTypeCtor(selectedPropIds, typeSharperAttr, model)),
                ResolveProperties(selectedPropIds, typeSharperAttr, model),
                TsList.Create(FromTypeCastOperator(info.Ref(), typeSharperAttr)))
            => SelectedPropIds = selectedPropIds;

        protected static TsTypeRef ExtractFromTypeRef(TsAttr typeSharperAttr) => typeSharperAttr.TypeArgs.Single();

        #endregion

        #region Private

        private static TsMethod FromTypeCastOperator(TsTypeRef targetType, TsAttr typeSharperAttr)
            => TsMethod.ImplicitCastOperator(
                ExtractFromTypeRef(typeSharperAttr),
                targetType,
                param => $"=> new({param.CsRef()});".Indent());

        private static TsCtor FromTypeCtor(TsList<TsId> selectedPropertyIds, TsAttr typeSharperAttr, TsModel model)
        {
            var fromTypePropertyLookup = FromTypePropertyLookup(typeSharperAttr, model);
            var csPrimaryCtorArgs =
                selectedPropertyIds
                    .Select(propId => fromTypePropertyLookup[propId].CsGetFrom("fromValue"))
                    .JoinList();
            return TsCtor.Public(
                $": this({csPrimaryCtorArgs}) {{ }}",
                new TsParam(ExtractFromTypeRef(typeSharperAttr), "fromValue"));
        }

        private static TsDict<TsId, TsProp> FromTypePropertyLookup(TsAttr typeSharperAttr, TsModel model)
            => model.Resolve(ExtractFromTypeRef(typeSharperAttr)).Props.ToDictionary(prop => prop.Id);

        private static TsList<TsProp> ResolveProperties(TsList<TsId> propertyIds, TsAttr typeSharperAttr, TsModel model)
        {
            var lookup = FromTypePropertyLookup(typeSharperAttr, model);
            return propertyIds.Select(propId => lookup[propId]);
        }

        #endregion
    }

    #endregion
}
