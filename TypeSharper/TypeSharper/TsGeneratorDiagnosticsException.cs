using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TypeSharper.Support;

namespace TypeSharper;

public class TsGeneratorDiagnosticsException : Exception
{
    public TsGeneratorDiagnosticsException(IEnumerable<Diagnostic> diagnostics) => Diagnostics = diagnostics.ToList();
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public override string Message => Diagnostics.Select(d => d.ToString()).JoinLines();
}
