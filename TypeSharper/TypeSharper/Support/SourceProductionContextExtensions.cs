using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace TypeSharper.Support;

public static class SourceProductionContextExtensions
{
    public static void AddSource(
        this SourceProductionContext sourceProductionContext,
        string fileNameHint,
        SyntaxNode syntaxNode)
        => sourceProductionContext.AddSource(
            $"TypeSharp.{fileNameHint}.g.cs",
            Formatter.Format(syntaxNode, new AdhocWorkspace()).ToFullString());
}
