using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Identifier;
using TypeSharper.Model.Type;

namespace TypeSharper.Diagnostics;

public static class Diag
{
    public static void ReportError(
        this EDiagnosticsCode diagnosticsCode,
        SourceProductionContext context,
        string messageFormat,
        params object?[]? messageArgs)
    {
        diagnosticsCode.ReportError(context, messageFormat, null, messageArgs);
    }

    public static void ReportError(
        this EDiagnosticsCode diagnosticsCode,
        SourceProductionContext context,
        string messageFormat,
        Location? location = null,
        params object?[]? messageArgs)
    {
        var diagnosticDescriptor =
            new DiagnosticDescriptor(
                "TYS" + $"{diagnosticsCode:D}".PadLeft(4, '0'),
                $"{diagnosticsCode:G}",
                messageFormat,
                "TypeSharper",
                DiagnosticSeverity.Error,
                true);

        context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, location, messageArgs));
    }

    public static bool RunPropertyDoesNotExistDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsType type,
        IEnumerable<TsId> propertyNames)
    {
        var typePropLookup = type.Props.ToDictionary(prop => prop.Id);
        var nonExistingPropertyNames =
            propertyNames.Where(propName => !typePropLookup.ContainsKey(propName)).ToList();

        if (nonExistingPropertyNames.Count == 0)
        {
            return true;
        }

        EDiagnosticsCode.PropertyDoesNotExist.ReportError(
            sourceProductionContext,
            "The following properties do not exist on type {0}: ({1})",
            type.Ref().Cs(),
            nonExistingPropertyNames.Select(id => id.Cs()).JoinList());

        return false;
    }

    public static bool RunTypeHierarchyIsPartialDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType type)
    {
        if (type.Mods.Partial.IsSet)
        {
            return type.Mods.Partial.IsSet
                   && type.Info.ContainingType.Match(
                       containingType => RunTypeHierarchyIsPartialDiagnostics(
                           sourceProductionContext,
                           model,
                           model.Resolve(containingType)),
                       () => true);
        }

        EDiagnosticsCode.TypeHierarchyMustBePartial.ReportError(
            sourceProductionContext,
            "The type {0} must be partial.",
            type.Ref().Cs());

        return false;
    }

    public static bool RunTypeIsAbstractDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsType type)
    {
        if (type.Mods.Abstract.IsSet)
        {
            return true;
        }

        EDiagnosticsCode.TypeMustBeAbstract.ReportError(
            sourceProductionContext,
            "The type {0} must be abstract.",
            type);

        return false;
    }

    public static bool RunTypeIsRecordDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsType type)
    {
        if (type.TypeKind is TsType.EKind.RecordClass or TsType.EKind.RecordStruct)
        {
            return true;
        }

        EDiagnosticsCode.TargetTypeMustBeRecord.ReportError(
            sourceProductionContext,
            "The type {0} must be a record.",
            type);

        return false;
    }
}
