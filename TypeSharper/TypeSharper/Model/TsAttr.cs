using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeSharper.Diagnostics;
using TypeSharper.SemanticExtensions;

namespace TypeSharper.Model;

public abstract record TsAttr
{
    public const string NS = "TypeSharper.Attributes";

    public Maybe<DiagnosticsError> RunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model)
        => TargetTypeIsErrorSymbol(targetTypeSymbol)
           .IfNone(() => TargetTypeHierarchyIsNotPartial(targetTypeSymbol))
           .IfNone(() => TargetTypeIsRecord(targetTypeSymbol))
           .IfNone(() => DoRunDiagnostics(targetTypeSymbol, model));

    private static Maybe<DiagnosticsError> TargetTypeIsRecord(ITypeSymbol typeSymbol)
        => typeSymbol.DeclaringSyntaxReferences.First().GetSyntax().Kind() is not SyntaxKind.RecordDeclaration
            and not SyntaxKind.RecordStructDeclaration
            ? new DiagnosticsError(
                EDiagnosticsCode.TargetTypeIsNotARecord,
                typeSymbol,
                "The type {0} must be a record."
                + " Currently only records are supported as target types for TypeSharper."
                + " This might change in the future but currently this makes everything easier since only one syntax"
                + " has to be supported.",
                typeSymbol)
            : Maybe<DiagnosticsError>.NONE;

    private static Maybe<DiagnosticsError> TargetTypeIsErrorSymbol(ISymbol symbol)
        => symbol is IErrorTypeSymbol
            ? new DiagnosticsError(
                EDiagnosticsCode.TargetTypeSymbolHasError,
                symbol,
                "The type {0} has some error. All errors must be resolved before code can be generated.",
                symbol)
            : Maybe<DiagnosticsError>.NONE;

    private static Maybe<DiagnosticsError> TargetTypeHierarchyIsNotPartial(ITypeSymbol typeSymbol)
        => typeSymbol.IsPartial()
            ? typeSymbol.ContainingType is not null
                ? TargetTypeHierarchyIsNotPartial(typeSymbol.ContainingType)
                : Maybe<DiagnosticsError>.NONE
            : new DiagnosticsError(
                EDiagnosticsCode.TargetTypeHierarchyIsNotPartial,
                typeSymbol,
                "The type {0} must be partial. All types decorated"
                + " with a [Ts...] attribute must be partial in order for"
                + " TypeSharper to generate a partial that implements the type.",
                typeSymbol);

    protected abstract Maybe<DiagnosticsError> DoRunDiagnostics(ITypeSymbol targetTypeSymbol, TsModel model);
}
