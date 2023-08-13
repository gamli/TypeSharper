using System;
using TypeSharper.Diagnostics;

namespace TypeSharper.Generator;

public class TsGeneratorException : Exception
{
    public TsGeneratorException(EDiagnosticsCode code, string fmtMsg, params object[] fmtArgs)
        : this(null, code, fmtMsg, fmtArgs) { }

    public TsGeneratorException(Exception? innerExc, EDiagnosticsCode code, string fmtMsg, params object[] fmtArgs)
        : base(string.Format(fmtMsg, fmtArgs), innerExc)
    {
        Code = code;
        FmtMsg = fmtMsg;
        FmtArgs = fmtArgs;
    }

    public EDiagnosticsCode Code { get; }

    public object[] FmtArgs { get; }
    public string FmtMsg { get; }
}
