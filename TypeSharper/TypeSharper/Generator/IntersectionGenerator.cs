using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Modifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public class IntersectionGenerator : TypeGenerator
{
    public override TsAttrDef AttributeDefinition(IncrementalGeneratorInitializationContext context)
        => new(
            new TsId("TypeSharperIntersectionAttribute"),
            AttributeTargets.Interface | AttributeTargets.Class,
            TsList.Create(
                Enumerable
                    .Range(1, 10)
                    .Select(
                        parameterCount =>
                            new TsAttrOverloadDef(
                                new TsList<TsParam>(),
                                new TsList<TsParam>(),
                                TsList.Create(
                                    Enumerable
                                        .Range(0, parameterCount)
                                        .Select(parameterIdx => new TsId($"TType_{parameterIdx}")))))));

    public override bool RunDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType targetType,
        TsAttr attr)
        => true;

    #region Protected

    protected override TsModel DoGenerate(TsType targetType, TsAttr attr, TsModel model)
    {
        var newPartial = targetType.NewPartial();

        var props = PropsPresentInAllTypes(attr, model).ToList();

        if (newPartial.TypeKind is TsType.EKind.RecordClass or TsType.EKind.RecordStruct)
        {
            newPartial = newPartial.SetPrimaryCtor(GeneratePrimaryCtor(props));
        }

        var intersectionType =
            newPartial
                .AddCtors(GenerateCtors(props, newPartial, attr))
                .AddProps(props)
                .AddMethods(GenerateCastOperators(props, newPartial, attr));

        return model.AddType(intersectionType);
    }

    #endregion

    #region Private

    private static TsList<TsTypeRef> ConstituentTypes(TsAttr attr) => attr.TypeArgs;

    private static IEnumerable<TsMethod> GenerateCastOperators(
        IEnumerable<TsProp> props,
        TsType targetType,
        TsAttr attr)
    {
        return ConstituentTypes(attr)
            .Select(
                type => new TsMethod(
                    new TsId(type.Id.Cs().Replace(".", "_")),
                    targetType.Ref(),
                    new TsList<TsParam>(new TsParam(type, new TsId("valueToCast"), false)),
                    new TsMemberMods(
                        ETsVisibility.Public,
                        new TsAbstractMod(false),
                        new TsStaticMod(true),
                        ETsOperator.Implicit),
                    Maybe.Some(
                        targetType.PrimaryCtor.Match(
                            ctor =>
                                $"=> new ({props.Select(prop => prop.CsGetFrom("valueToCast")).JoinList()});"
                                    .Indent(),
                            () =>
                                $$"""
                                    => new {{targetType.Id.Cs()}}
                                    {
                                    {{props.Select(prop => prop.CsSet(prop.CsGetFrom("valueToCast"))).JoinList()}}
                                    };
                                    """.Indent()))));
    }

    private static IEnumerable<TsCtor> GenerateCtors(
        IEnumerable<TsProp> props,
        TsType targetType,
        TsAttr attr)
        => ConstituentTypes(attr)
            .Select(
                typeRef =>
                {
                    return (TsCtor)new TsCtor(
                        new TsList<TsParam>(new TsParam(typeRef, new TsId("valueToConvert"), false)),
                        new TsMemberMods(
                            ETsVisibility.Public,
                            new TsAbstractMod(false),
                            new TsStaticMod(false),
                            ETsOperator.None),
                        Maybe.Some(
                            targetType
                                .PrimaryCtor
                                .Match(
                                    _ =>
                                        $": this({props.Select(prop => prop.CsGetFrom("valueToConvert")).JoinList()}) {{ }}",
                                    () =>
                                        $$"""
                                        {
                                        {{props.Select(prop => prop.CsSet(prop.CsGetFrom("valueToConvert")) + ";").JoinLines()}}
                                        }
                                        """)));
                });


    private static TsPrimaryCtor GeneratePrimaryCtor(IEnumerable<TsProp> props)
        => new(TsList.Create(props.Select(prop => new TsParam(prop.Type, prop.Id, false))));

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
