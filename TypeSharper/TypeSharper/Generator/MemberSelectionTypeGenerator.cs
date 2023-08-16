using System;
using System.Collections.Generic;
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
            AttributeTargets.Interface | AttributeTargets.Class,
            TsList.Create(
                new TsAttrOverloadDef(
                    TsList.Create(
                        new TsParam(
                            new TsTypeRef(
                                TsNs.Qualified(new TsQualifiedId("System")),
                                new TsQualifiedId("String"),
                                true),
                            new TsId("memberNames"),
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
            SelectedPropertyNames(attr));

    #region Protected

    protected abstract TsId AttributeId();

    protected abstract TsModel DoGenerate(
        TsModel model,
        TsType fromType,
        TsDict<TsId, TsProp> fromTypePropertyLookup,
        TsList<TsId> selectedPropertyNames,
        TsType targetType,
        TsAttr attr);

    protected override TsModel DoGenerate(TsType targetType, TsAttr attr, TsModel model)
    {
        var fromType = FromType(attr, model);

        var fromTypePropertyLookup = FromTypePropertyLookup(fromType);

        var selectedPropertyNames = SelectedPropertyNames(attr);

        return DoGenerate(
            model,
            fromType,
            fromTypePropertyLookup,
            selectedPropertyNames,
            targetType,
            attr);
    }

    #endregion

    #region Private

    private static TsType FromType(TsAttr attr, TsModel model) => model.Resolve(attr.TypeArgs.Single());

    private static TsDict<TsId, TsProp> FromTypePropertyLookup(TsType fromType)
        => fromType.Props.ToDictionary(prop => prop.Id);

    private static TsList<TsId> SelectedPropertyNames(TsAttr attr)
        => TsList.Create(
            attr
                .CtorArgs
                .SelectMany(
                    attrValue =>
                        attrValue
                            .Match(
                                primitive => new[] { primitive },
                                array => (IEnumerable<string>)array)
                            .Select(propertyName => new TsId(propertyName))));

    #endregion
}
