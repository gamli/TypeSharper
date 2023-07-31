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

    public override TsModel Generate(TsType targetType, TsAttr attr, TsModel model)
        => Generate(
            model,
            model.Resolve(attr.TypeArgs.First()),
            model.Resolve(attr.TypeArgs.Single()).Props.ToDictionary(prop => prop.Id),
            TsList.Create(
                attr
                    .CtorArgs
                    .SelectMany(
                        attrValue =>
                            attrValue
                                .Match(
                                    primitive => new[] { primitive },
                                    array => (IEnumerable<string>)array)
                                .Select(propertyName => new TsId(propertyName)))),
            targetType,
            attr);

    public override bool RunDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType targetType,
        TsAttr attr)
        => EDiagnosticsCode.TypeMustBeInterfaceOrClass.RunDiagnostics(
            sourceProductionContext,
            model,
            model.Resolve(attr.TypeArgs.Single()));

    #region Protected

    protected abstract TsId AttributeId();

    protected abstract TsModel Generate(
        TsModel model,
        TsType fromType,
        TsDict<TsId, TsProp> fromTypePropertyLookup,
        TsList<TsId> selectedPropertyNames,
        TsType targetType,
        TsAttr attr);

    #endregion
}
