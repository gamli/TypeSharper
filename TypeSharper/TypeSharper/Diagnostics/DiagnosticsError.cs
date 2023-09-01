using System.Linq;
using Microsoft.CodeAnalysis;

namespace TypeSharper.Diagnostics;

public class DiagnosticsError
{
    public DiagnosticsError(
        EDiagnosticsCode diagnosticsCode,
        Location? location,
        string messageFormat,
        params object[] messageArgs)
    {
        _diagnosticsCode = diagnosticsCode;
        _location = location;
        _messageArgs = messageArgs;
        _messageFormat = messageFormat;
    }

    public DiagnosticsError(
        EDiagnosticsCode diagnosticsCode,
        ISymbol? symbol,
        string messageFormat,
        params object[] messageArgs)
    {
        _diagnosticsCode = diagnosticsCode;
        _messageArgs = messageArgs;
        _messageFormat = messageFormat;
        _symbol = symbol;
    }

    public DiagnosticsError(
        EDiagnosticsCode diagnosticsCode,
        string messageFormat,
        params object[] messageArgs)
    {
        _diagnosticsCode = diagnosticsCode;
        _messageArgs = messageArgs;
        _messageFormat = messageFormat;
    }

    public void Report(SourceProductionContext context)
    {
        var diagnosticDescriptor =
            new DiagnosticDescriptor(
                "TYS" + $"{_diagnosticsCode:D}".PadLeft(4, '0'),
                $"{_diagnosticsCode:G}",
                _messageFormat,
                "TypeSharper",
                DiagnosticSeverity.Error,
                true);

        context.ReportDiagnostic(
            Diagnostic.Create(diagnosticDescriptor, _location ?? _symbol?.Locations.FirstOrDefault(), _messageArgs));
    }

    #region Private

    private readonly EDiagnosticsCode _diagnosticsCode;
    private readonly Location? _location;
    private readonly object[] _messageArgs;
    private readonly string _messageFormat;
    private readonly ISymbol? _symbol;

    #endregion
}
