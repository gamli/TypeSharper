using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using TypeSharper.Diagnostics;

namespace TypeSharper.Tests.Generator;

public static class GeneratorTest
{
    public static GeneratorDriverRunResult ExpectEmptyOutput(string input)
        => ExpectOutput(input, Array.Empty<(string fileName, IEnumerable<string> expectedCode)>());

    public static GeneratorDriverRunResult ExpectOutput(string input, params string[] expectedOutputs)
        => ExpectOutput(input, (".g.cs", expectedOutputs));

    public static GeneratorDriverRunResult ExpectOutput(
        string input,
        params (string fileName, string expectedCode)[] expectedFiles)
        => ExpectOutput(
            input,
            expectedFiles.Select(t => (t.fileName, (IEnumerable<string>)new[] { t.expectedCode })).ToArray());

    public static GeneratorDriverRunResult ExpectOutput(
        string input,
        params (string fileName, IEnumerable<string> expectedCodes)[] expectedFiles)
    {
        var result = Succeed(input);

        var userGeneratedFiles =
            result
                .GeneratedTrees
                .Where(tree => !tree.FilePath.EndsWith("Attribute.g.cs") && !tree.FilePath.Contains("Support/"))
                .ToList();

        userGeneratedFiles.Should().HaveSameCount(expectedFiles);

        foreach (var t in expectedFiles)
        {
            var (expectedFileName, expectedCodes) = t;

            userGeneratedFiles
                .Select(tree => tree.FilePath)
                .Should()
                .Contain(path => path.EndsWith(expectedFileName));

            var generatedSrc =
                userGeneratedFiles
                    .Single(tree => tree.FilePath.EndsWith(expectedFileName))
                    .GetText()
                    .ToString()
                    .ReplaceLineEndings("\n");

            foreach (var expectedCode in expectedCodes)
            {
                var expectedSrc =
                    Formatter
                        .Format(
                            CSharpSyntaxTree.ParseText(expectedCode).GetRoot(),
                            new AdhocWorkspace())
                        .ToFullString();

                generatedSrc.Should().ContainIgnoreWhitespace(expectedSrc);
            }
        }

        return result;
    }

    public static GeneratorDriverRunResult Fail(EDiagnosticsCode code, IEnumerable<string> sources)
        => Fail(code, sources.ToArray());

    public static GeneratorDriverRunResult Fail(EDiagnosticsCode code, params string[] sources)
    {
        var result = Run(sources);
        result
            .Diagnostics
            .Should()
            .Contain(
                d => d.Severity == DiagnosticSeverity.Error
                     && d.Descriptor.Title.ToString() == $"{code:G}");
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

file static class GeneratorStringAssertionsExtensions
{
    public static GeneratorStringAssertions Should(this string instance) => new(instance);
}

file class GeneratorStringAssertions : ReferenceTypeAssertions<string, GeneratorStringAssertions>
{
    public GeneratorStringAssertions(string instance)
        : base(instance) { }

    public AndConstraint<GeneratorStringAssertions> ContainIgnoreWhitespace(
        string expected,
        string because = "",
        params object[] becauseArgs)
    {
        Execute
            .Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(NormalizeWhitespace(Subject).Contains(NormalizeWhitespace(expected)))
            .FailWith(
                "Expected {0} to contain {1}{reason}, it did not. Here is the diff:\n{2}",
                Subject,
                expected,
                Diffplex.DiffString(Subject, expected));

        return new AndConstraint<GeneratorStringAssertions>(this);
    }

    #region Protected

    protected override string Identifier => "GeneratorStringAssertions";

    #endregion

    #region Private

    private static readonly Regex _whitespaceRegex = new("\\s+");

    private static string NormalizeWhitespace(string str) => _whitespaceRegex.Replace(str, "");

    #endregion
}
