using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeSharper.Support;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
        => enumerable.SelectMany(x => x);

    public static IEnumerable<T> Generate<T>(int count, Func<int, T> generateNextValue)
    {
        for (var i = 0; i < count; i++)
        {
            yield return generateNextValue(i);
        }
    }

    public static IEnumerable<T> Repeat<T>(this T value)
    {
        while (true)
        {
            yield return value;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public static IEnumerable<T> Repeat<T>(this T value, int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return value;
        }
    }

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable)
    {
        var prevSet = false;
        var prev = default(T);
        foreach (var element in enumerable)
        {
            if (prevSet)
            {
                yield return prev!;
            }

            prev = element;
            prevSet = true;
        }
    }
}
