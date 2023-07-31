using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace TypeSharper.Tests.Generator;

public static class GeneratorTest
{
    public static GeneratorDriverRunResult ExpectOutput(string input, string expectedOutput)
        => ExpectOutput(input, (".g.cs", expectedOutput));

    public static GeneratorDriverRunResult ExpectOutput(
        string input,
        params (string fileName, string expectedCode)[] expectedFiles)
    {
        var result = Succeed(input);

        var generatedFiles = result.GeneratedTrees;

        expectedFiles
            .Should()
            .AllSatisfy(
                t =>
                {
                    var (expectedFileName, expectedCode) = t;

                    generatedFiles
                        .Select(tree => tree.FilePath)
                        .Should()
                        .Contain(
                            path => path.EndsWith(expectedFileName)
                                    && !path.EndsWith("Attribute.g.cs"));

                    var generatedSrc =
                        generatedFiles
                            .Single(
                                tree => tree.FilePath.EndsWith(expectedFileName)
                                        && !tree.FilePath.EndsWith("Attribute.g.cs"))
                            .GetText()
                            .ToString()
                            .ReplaceLineEndings("\n");

                    var expectedSrc =
                        Formatter
                            .Format(
                                CSharpSyntaxTree.ParseText(expectedCode).GetRoot(),
                                new AdhocWorkspace())
                            .ToFullString()
                            .ReplaceLineEndings("\n");

                    StringDiff.Print(generatedSrc, expectedSrc);

                    generatedSrc.Should().Contain(expectedSrc);
                });

        return result;
    }

    public static GeneratorDriverRunResult Fail(IEnumerable<string> sources) => Fail(sources.ToArray());

    public static GeneratorDriverRunResult Fail(params string[] sources)
    {
        var result = Run(sources);
        result.Diagnostics.Should().Contain(d => d.Severity == DiagnosticSeverity.Error);
        return result;
    }

    public static GeneratorDriverRunResult Run(IEnumerable<string> sources)
    {
        var generator = new TypeSharperGenerator();

        var firstPathCompilation = CSharpCompilation.Create(
            generator.GetType().Name,
            sources.Select(src => CSharpSyntaxTree.ParseText(src)),
            Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(assemblyName => MetadataReference.CreateFromFile(Assembly.Load(assemblyName).Location))
                .Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)));

        var driver =
            CSharpGeneratorDriver
                .Create(generator)
                .RunGeneratorsAndUpdateCompilation(
                    firstPathCompilation,
                    out var secondPassCompilation,
                    out var secondPassDiagnostics);

        return driver.GetRunResult();
    }

    public static GeneratorDriverRunResult Succeed(IEnumerable<string> sources) => Succeed(sources.ToArray());

    public static GeneratorDriverRunResult Succeed(params string[] sources)
    {
        var result = Run(sources);
        result.Diagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        return result;
    }
}
