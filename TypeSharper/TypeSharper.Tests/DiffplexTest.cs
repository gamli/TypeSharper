using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using FluentAssertions;
using Xunit;

namespace TypeSharper.Tests;

public class DiffplexTest
{
    [Fact]
    public void InlineDiffBuilder_includes_unchanged_lines_by_default()
        => TestDiff("hello", "hello", (ChangeType.Unchanged, "hello"));

    [Fact]
    public void InlineDiffBuilder_treats_lines_present_in_both_but_changed_as_deleted_and_then_inserted()
        => TestDiff("helo", "hello", (ChangeType.Deleted, "helo"), (ChangeType.Inserted, "hello"));

    [Fact]
    public void InlineDiffBuilder_treats_lines_present_only_in_new_text_as_inserted()
        => TestDiff("", "hello", (ChangeType.Inserted, "hello"));

    [Fact]
    public void InlineDiffBuilder_treats_lines_present_only_in_old_text_as_deleted()
        => TestDiff("hello", "", (ChangeType.Deleted, "hello"));

    #region Private

    private static void TestDiff(
        string oldText,
        string newText,
        params (ChangeType type, string? text)[] expectedDiffLines)
    {
        var diff = InlineDiffBuilder.Diff(oldText, newText);
        foreach (var (type, text) in expectedDiffLines)
        {
            diff
                .Lines
                .Should()
                .Contain(diffPiece => diffPiece.Type == type && (text == null || diffPiece.Text == text));
        }
    }

    #endregion
}
