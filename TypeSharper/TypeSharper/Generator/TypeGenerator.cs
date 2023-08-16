using System;
using Microsoft.CodeAnalysis;
using TypeSharper.Diagnostics;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public abstract class TypeGenerator
{
    public abstract TsAttrDef AttributeDefinition(IncrementalGeneratorInitializationContext context);

    public abstract bool RunDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType targetType,
        TsAttr attr);

    public TsModel Generate(TsType targetType, TsAttr attr, TsModel model)
    {
        try
        {
            return DoGenerate(targetType, attr, model);
        }
        catch (Exception e)
        {
            throw new TsGeneratorException(
                e,
                EDiagnosticsCode.UnknownGeneratorError,
                "Unknown generator error: " + e.Message);
        }
    }

    #region Protected

    protected abstract TsModel DoGenerate(TsType targetType, TsAttr attr, TsModel model);

    #endregion
}
