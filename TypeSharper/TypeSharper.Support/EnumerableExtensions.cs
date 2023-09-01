using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeSharper;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Concat<T>(this T firstElement, IEnumerable<T> restEnumerable)
        => new[] { firstElement }.Concat(restEnumerable);
    public static IEnumerable<T> Append<T>(this T firstElement, T secondElement)
        => new[] { firstElement }.Append(secondElement);

    public static IEnumerable<T> SelectIf<T>(
        this IEnumerable<T> enumerable,
        bool condition,
        Func<T, T> selector)
        => condition ? enumerable.Select(selector) : enumerable;

    public static IEnumerable<TResult> SelectIfElse<T, TResult>(
        this IEnumerable<T> enumerable,
        bool condition,
        Func<T, TResult> trueSelector,
        Func<T, TResult> falseSelector)
        => condition ? enumerable.Select(trueSelector) : enumerable.Select(falseSelector);

    public static IEnumerable<T> ContextWhere<T>(
        this IEnumerable<T> enumerable,
        int contextSize,
        Func<T, bool> filterPredicate)
    {
        var prefixContextQueue = new Queue<T>();
        var suffixContentCount = 0;

        using var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (filterPredicate(enumerator.Current))
            {
                foreach (var contextItem in prefixContextQueue)
                {
                    yield return contextItem;
                }

                prefixContextQueue.Clear();
                yield return enumerator.Current;
                suffixContentCount = contextSize;
            }
            else
            {
                if (suffixContentCount != 0)
                {
                    yield return enumerator.Current;
                    suffixContentCount--;
                }
                else
                {
                    prefixContextQueue.Enqueue(enumerator.Current);
                    if (prefixContextQueue.Count > contextSize)
                    {
                        prefixContextQueue.Dequeue();
                    }
                }
            }
        }
    }

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

    public static IEnumerable<T> SelectWhereSome<TSource, T>(
        this IEnumerable<TSource> enumerable,
        Func<TSource, Maybe<T>> selector)
        => enumerable.Select(selector).WhereSome();
    
    public static IEnumerable<TSource> WhereSome<TSource>(this IEnumerable<Maybe<TSource>> enumerable)
        => enumerable
           .Where(element => element.Map())
           .Select(element => element.AssertSome());

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
    
    public static IEnumerable<(T first, T? second)> Pairs<T>(this IEnumerable<T> enumerable)
    {
        using var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var first = enumerator.Current;
            var second = enumerator.MoveNext() ? enumerator.Current : default;
            yield return (first, second);
        }
    }
}
