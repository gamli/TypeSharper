using System;
using Microsoft.CodeAnalysis;
using TypeSharper.Model;
using TypeSharper.Model.Type;

namespace TypeSharper;

public enum EDiagnosticsCode { TypeHierarchyMustBePartial, TypeMustBeInterfaceOrClass, TypeMustBeAbstract }

public static class DiagnosticCodeExtensions
{
    public static bool RunDiagnostics(
        this EDiagnosticsCode diagnosticsCode,
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType type)
        => diagnosticsCode switch
        {
            EDiagnosticsCode.TypeHierarchyMustBePartial
                => RunTypeHierarchyIsPartialDiagnostics(sourceProductionContext, model, type),
            EDiagnosticsCode.TypeMustBeInterfaceOrClass
                => RunTypeIsInterfaceOrClassDiagnostics(sourceProductionContext, type),
            EDiagnosticsCode.TypeMustBeAbstract
                => RunTypeIsAbstractDiagnostics(sourceProductionContext, type),
            _ => throw new ArgumentOutOfRangeException(nameof(diagnosticsCode), diagnosticsCode, null),
        };

    #region Private

    private static void ReportError(
        this EDiagnosticsCode diagnosticsCode,
        SourceProductionContext context,
        string messageFormat,
        params object?[]? messageArgs)
    {
        diagnosticsCode.ReportError(context, messageFormat, null, messageArgs);
    }

    private static void ReportError(
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

    private static bool RunTypeHierarchyIsPartialDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsModel model,
        TsType type)
    {
        if (type.Mods.Partial.IsSet)
        {
            return type.Mods.Partial.IsSet
                   && type.ContainingType.Match(
                       containingType => RunTypeHierarchyIsPartialDiagnostics(
                           sourceProductionContext,
                           model,
                           model.Resolve(containingType)),
                       () => true);
        }

        EDiagnosticsCode.TypeHierarchyMustBePartial.ReportError(
            sourceProductionContext,
            "The type {0} must be partial.",
            type);

        return false;
    }

    private static bool RunTypeIsAbstractDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsType type)
    {
        if (type.Mods.Abstract.IsSet)
        {
            return true;
        }

        EDiagnosticsCode.TypeHierarchyMustBePartial.ReportError(
            sourceProductionContext,
            "The type {type} must be abstract.",
            type);

        return false;
    }

    private static bool RunTypeIsInterfaceOrClassDiagnostics(
        SourceProductionContext sourceProductionContext,
        TsType type)
    {
        if (type.TypeKind is TsType.EKind.Interface or TsType.EKind.Class)
        {
            return true;
        }

        EDiagnosticsCode.TypeMustBeInterfaceOrClass.ReportError(
            sourceProductionContext,
            "The type {type} must be a an interface or a class (not a record).",
            type);

        return false;
    }

    #endregion
}
