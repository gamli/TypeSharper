using System;
using System.Collections.Generic;
using System.Linq;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace TypeSharper.Tests;

public class Diffplex
{
    public static IEnumerable<DiffPiece> Diff(string str1, string str2, params ChangeType[] changeTypesToIgnore)
        => InlineDiffBuilder
           .Diff(str1, str2)
           .Lines
           .Where(line => !changeTypesToIgnore.Contains(line.Type))
           .ContextWhere(4, line => line.Type != ChangeType.Unchanged);

    public static string DiffString(string str1, string str2, params ChangeType[] changeTypesToIgnore)
        => Diff(str1, str2, changeTypesToIgnore)
           .Select(line => $"{ChangeTypeSymbol(line.Type)} {line.Text}")
           .JoinLines();

    public static void Print(string str1, string str2, params ChangeType[] changeTypesToIgnore)
    {
        var savedColor = Console.ForegroundColor;

        foreach (var line in Diff(str1, str2, changeTypesToIgnore))
        {
            Console.ForegroundColor = ChangeTypeColor(line.Type);
            Console.Write($"{ChangeTypeSymbol(line.Type)} {line.Text}");
        }

        Console.ForegroundColor = savedColor;
    }

    public static string UnifiedDiff(string str1, string str2)
    {
        var diff = InlineDiffBuilder.Diff(str1, str2);

        foreach (var line in diff.Lines)
        {
            Console.WriteLine(line.Text);
        }

        return diff
               .Lines
               .Select(
                   line => line.Type switch
                   {
                       ChangeType.Inserted  => "+ " + line.Text,
                       ChangeType.Deleted   => "- " + line.Text,
                       ChangeType.Modified  => "~ " + line.Text,
                       ChangeType.Imaginary => "? " + line.Text,
                       _                    => "  " + line.Text,
                   })
               .JoinLines();
    }

    #region Private

    private static ConsoleColor ChangeTypeColor(ChangeType changeType)
        => MatchChangeType(
            changeType,
            ConsoleColor.Gray,
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.DarkYellow,
            ConsoleColor.Blue);

    private static string ChangeTypeSymbol(ChangeType changeType)
        => MatchChangeType(changeType, " ", "-", "+", "?", "~");

    private static T MatchChangeType<T>(
        ChangeType changeType,
        T unchanged,
        T deleted,
        T inserted,
        T imaginary,
        T modified)
        => changeType switch
        {
            ChangeType.Unchanged => unchanged,
            ChangeType.Deleted   => deleted,
            ChangeType.Inserted  => inserted,
            ChangeType.Imaginary => imaginary,
            ChangeType.Modified  => modified,
            _                    => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null),
        };

    #endregion
}
