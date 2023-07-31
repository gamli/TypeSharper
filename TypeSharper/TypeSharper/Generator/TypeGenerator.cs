using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Attr;
using TypeSharper.Model.Attr.Def;
using TypeSharper.Model.Type;

namespace TypeSharper.Generator;

public abstract class TypeGenerator
{
    public abstract TsAttrDef AttributeDefinition(IncrementalGeneratorInitializationContext context);

    public abstract TsModel Generate(TsType targetType, TsAttr attr, TsModel model);

    public virtual bool RunDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType targetType,
        TsAttr attr)
        => true;
}
