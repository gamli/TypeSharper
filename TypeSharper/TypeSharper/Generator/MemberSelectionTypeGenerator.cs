using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Member;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public abstract class MemberSelectionTypeGenerator : TypeGenerator
{
    public override TsAttrDef AttributeDefinition(IncrementalGeneratorInitializationContext context)
        => new(
            AttributeId(),
            AttributeTargets.Class | AttributeTargets.Struct,
            TsList.Create(
                new TsAttrOverloadDef(
                    TsList.Create(
                        new TsParam(
                            TsTypeRef.WithNsArray("System", "String"),
                            "memberNames",
                            true)),
                    TsList<TsParam>.Empty,
                    TsList.Create(new TsId("TFromType")))));

    public override bool RunDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType targetType,
        TsAttr attr)
        => Diag.RunPropertyDoesNotExistDiagnostics(
            sourceProductionContext,
            FromType(attr, model),
            attr.FlattenedArgs().Select(arg => new TsId(arg)));

    #region Protected

    protected abstract TsId AttributeId();

    #endregion

    #region Private

    private static TsType FromType(TsAttr attr, TsModel model) => model.Resolve(attr.TypeArgs.Single());

    #endregion
}
