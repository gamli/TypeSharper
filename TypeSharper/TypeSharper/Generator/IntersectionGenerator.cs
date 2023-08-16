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
using TypeSharper.Support;

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
        var propsPresentInAllTypes = PropsPresentInAllTypes(attr, model).ToList();

        var intersectionType =
            targetType
                .NewPartial()
                .AddCtors(GenerateCtors(propsPresentInAllTypes, attr, model))
                .AddProps(propsPresentInAllTypes);

        if (intersectionType.TypeKind is TsType.EKind.RecordClass or TsType.EKind.RecordStruct)
        {
            intersectionType =
                intersectionType.SetPrimaryCtor(GeneratePrimaryCtor(propsPresentInAllTypes));
        }

        return model.AddType(intersectionType);
    }

    #endregion

    #region Private

    private static TsList<TsTypeRef> ConstituentTypes(TsAttr attr) => attr.TypeArgs;

    private static TsCtor GenerateConstructorForConstituentType(TsType type, IEnumerable<TsProp> propsPresentInAllTypes)
        => new(
            new TsList<TsParam>(new TsParam(type.Ref(), new TsId("value"), false)),
            new TsMemberMods(
                ETsVisibility.Public,
                new TsAbstractMod(false),
                new TsStaticMod(false)),
            Maybe.Some(
                type.PrimaryCtor.Match(
                    ctor =>
                        $": this({ctor.Params.Select(param => new TsQualifiedId("value", param.Id.Cs()).Cs()).JoinList().Indent()}) {{ }}",
                    () =>
                        $$"""
                        {
                        {{propsPresentInAllTypes.Select(prop => prop.CsAssign(new TsQualifiedId("value", prop.Id.Cs()).Cs())).JoinLines().Indent()}}
                        }
                        """)));

    private static IEnumerable<TsCtor> GenerateCtors(
        IEnumerable<TsProp> propsPresentInAllTypes,
        TsAttr attr,
        TsModel model)
        => ConstituentTypes(attr)
            .Select(type => GenerateConstructorForConstituentType(model.Resolve(type), propsPresentInAllTypes));


    private static TsPrimaryCtor GeneratePrimaryCtor(IEnumerable<TsProp> propsPresentInAllTypes)
        => new(TsList.Create(propsPresentInAllTypes.Select(prop => new TsParam(prop.Type, prop.Id, false))));

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
