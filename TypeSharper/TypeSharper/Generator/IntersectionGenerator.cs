using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
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

    #region Protected

    protected override TsModel DoGenerate(TsType targetType, TsAttr attr, TsModel model)
    {
        var intersectedTypeCount = attr.TypeArgs.Count;

        var propsPresentInAllTypes =
            attr
                .TypeArgs
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
                .Where(kv => kv.Value.count == intersectedTypeCount)
                .Select(kv => kv.Value.prop);

        return model.AddType(targetType.NewPartial().AddProps(propsPresentInAllTypes));
    }

    #endregion
}
