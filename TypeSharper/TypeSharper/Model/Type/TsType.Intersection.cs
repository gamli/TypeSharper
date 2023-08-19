using System.Collections.Generic;
using System.Linq;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;

namespace TypeSharper.Model.Type;

public partial interface TsType
{
    public static TsType CreateIntersection(TypeInfo info, TsAttr typeSharperAttr, TsModel model)
        => IntersectionImpl.Create(info, typeSharperAttr, model);

    #region Nested types

    public interface Intersection : Duck
    {
        public TsList<TsTypeRef> IntersectedTypes { get; }
    }

    private sealed record IntersectionImpl : DuckImpl, Intersection
    {
        public TsList<TsTypeRef> IntersectedTypes => TypeSharperAttr.TypeArgs;

        public static TsType Create(TypeInfo info, TsAttr typeSharperAttr, TsModel model)
        {
            var props = PropsPresentInAllTypes(typeSharperAttr, model).ToList();
            var intersectionType =
                new IntersectionImpl(info, typeSharperAttr)
                    .SetPrimaryCtor(GeneratePrimaryCtor(props));
            return intersectionType
                   .AddCtors(GenerateCtors(props, intersectionType, typeSharperAttr))
                   .AddProps(props)
                   .AddMethods(GenerateCastOperators(props, intersectionType, typeSharperAttr));
        }

        public override string ToString() => $"{base.ToString()}:Intersection";

        #region Private

        private IntersectionImpl(TypeInfo Info, TsAttr TypeSharperAttr)
            : base(Info, TypeSharperAttr, TypeSharperAttr.TypeArgs) { }

        private static TsList<TsTypeRef> ConstituentTypes(TsAttr attr) => attr.TypeArgs;

        private static IEnumerable<TsMethod> GenerateCastOperators(
            List<TsProp> props,
            TsType targetType,
            TsAttr attr)
        {
            return ConstituentTypes(attr)
                .Select(
                    type => new TsMethod(
                        new TsId(type.Cs().Replace(".", "_")),
                        targetType.Ref(),
                        new TsList<TsTypeRef>(),
                        new TsList<TsParam>(new TsParam(type, new TsId("valueToCast"))),
                        new TsMemberMods(
                            ETsVisibility.Public,
                            new TsAbstractMod(false),
                            new TsStaticMod(true),
                            ETsOperator.Implicit),
                        targetType.PrimaryCtor.Match(
                            _ =>
                                $"=> new ({props.Select(prop => prop.CsGetFrom("valueToCast")).JoinList()});"
                                    .Indent(),
                            () =>
                                $$"""
                                    => new {{targetType.Info.Id.Cs()}}
                                    {
                                    {{props.Select(prop => prop.CsSet(prop.CsGetFrom("valueToCast"))).JoinList()}}
                                    };
                                    """.Indent())));
        }

        private static IEnumerable<TsCtor> GenerateCtors(
            List<TsProp> props,
            TsType targetType,
            TsAttr attr)
            => ConstituentTypes(attr)
                .Select(
                    typeRef =>
                    {
                        return new TsCtor(
                            new TsList<TsParam>(new TsParam(typeRef, new TsId("valueToConvert"))),
                            new TsMemberMods(
                                ETsVisibility.Public,
                                new TsAbstractMod(false),
                                new TsStaticMod(false),
                                ETsOperator.None),
                            props.Count == 0
                                ? "{ }"
                                : targetType
                                  .PrimaryCtor
                                  .Match(
                                      _ =>
                                          $": this({props.Select(prop => prop.CsGetFrom("valueToConvert")).JoinList()}) {{ }}",
                                      () =>
                                          $$"""
                                          {
                                          {{props.Select(prop => prop.CsSet(prop.CsGetFrom("valueToConvert")) + ";").JoinLines()}}
                                          }
                                          """));
                    });

        private static Maybe<TsPrimaryCtor> GeneratePrimaryCtor(IEnumerable<TsProp> props)
            => TsPrimaryCtor.Create(TsList.Create(props.Select(prop => prop.Id)));

        private static IEnumerable<TsProp> PropsPresentInAllTypes(TsAttr attr, TsModel model)
            => ConstituentTypes(attr)
               .Select(type => model.Resolve(type).Props.ToLookup(m => m.Id))
               .Aggregate(
                   new Dictionary<TsId, (TsProp prop, int count)>(),
                   (acc, lookup) =>
                   {
                       foreach (var kv in lookup)
                       {
                           foreach (var prop in kv)
                           {
                               if (acc.TryGetValue(kv.Key, out var t))
                               {
                                   acc[kv.Key] = (t.prop, t.count + 1);
                               }
                               else
                               {
                                   acc.Add(kv.Key, (prop, 1));
                               }
                           }
                       }

                       return acc;
                   })
               .Where(kv => kv.Value.count == attr.TypeArgs.Count)
               .Select(kv => kv.Value.prop);

        #endregion
    }

    #endregion
}
