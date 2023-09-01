using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeSharper;

public static class StringExtensions
{
    public static string AddIfNotEmpty(this string? str, string left = " ", string right = " ")
        => IfNotEmpty(str, left + str + right, str ?? "");

    public static string AddLeftIfNotEmpty(this string? str, string left = " ") => AddIfNotEmpty(str, left, "");
    public static string AddRightIfNotEmpty(this string? str, string right = " ") => AddIfNotEmpty(str, "", right);

    public static string Capitalize(this string? str)
        => string.IsNullOrEmpty(str)
            ? ""
            : str!.ToUpperInvariant()[0] + str.Substring(1);

    public static string IfNotEmpty(this string? str, string ifStr, string elseStr = "")
        => string.IsNullOrEmpty(str) ? elseStr : ifStr;


    public static string Indent(this string? str, int numSpaces = 4)
        => string.IsNullOrEmpty(str)
            ? ""
            : str.Lines().Indent(numSpaces);

    public static string Indent(this IEnumerable<string> lines, int numSpaces = 4)
        => lines.Select(line => IndentLine(line, numSpaces)).JoinLines();

    public static string IndentLine(this string? line, int numSpaces = 4)
        => string.IsNullOrEmpty(line)
            ? ""
            : new string(' ', numSpaces) + line;

    public static string JoinLines(this IEnumerable<string> lines) => lines.JoinString("\n");

    public static string JoinList(this IEnumerable<string> elements)
    {
        var elementList = elements.ToList();
        var singleLine = elementList.JoinString(", ");
        return singleLine.Length > 120
            ? $"\n{elementList.JoinString(",\n").Indent()}"
            : singleLine;
    }

    public static string JoinPath(this IEnumerable<string> values) => string.Join("/", values).AddRightIfNotEmpty("/");

    public static string JoinTokens(this IEnumerable<string> tokens)
        => tokens.WhereNotNullOrWhitespace().JoinString(" ");

    public static string[] Lines(this string? str)
        => string.IsNullOrEmpty(str) ? Array.Empty<string>() : str!.Split('\n');

    public static string MarginRight(this string? str) => AddRightIfNotEmpty(str);

    public static string RepeatString(this string? str, int count)
        => string.IsNullOrEmpty(str) ? "" : str!.Repeat(count).JoinString("");

    public static string Semicolon(this string? str) => str.AddRightIfNotEmpty(";");

    public static IEnumerable<string> WhereNotNullOrWhitespace(this IEnumerable<string> tokens)
        => tokens.Where(token => !string.IsNullOrWhiteSpace(token));

    #region Private

    private static string JoinString(this IEnumerable<string> values, string separator)
        => string.Join(separator, values);

    #endregion
}
