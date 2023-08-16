using System;
using System.Linq;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace TypeSharper.Tests;

public class Diffplex
{
    public static void Print(string str1, string str2, params ChangeType[] changeTypesToIgnore)
    {
        var diff = InlineDiffBuilder.Diff(str1, str2);

        var savedColor = Console.ForegroundColor;
        foreach (var line
                 in diff
                    .Lines
                    .Where(line => !changeTypesToIgnore.Contains(line.Type))
                    .ContextWhere(4, line => line.Type != ChangeType.Unchanged))
        {
            switch (line.Type)
            {
                case ChangeType.Inserted:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("+ ");
                    break;
                case ChangeType.Deleted:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("- ");
                    break;
                case ChangeType.Modified:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("~ ");
                    break;
                case ChangeType.Imaginary:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("? ");
                    break;
                case ChangeType.Unchanged:
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("  ");
                    break;
            }

            Console.WriteLine(line.Text);
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
}
