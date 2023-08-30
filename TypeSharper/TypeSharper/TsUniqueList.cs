using System;
using System.Collections;
using System.Collections.Generic;

namespace TypeSharper;

public static class TsUniqueList
{
    public static TsUniqueList<T> Create<T>(IEnumerable<T> enumerable) => new(TsList.Create(enumerable));
    public static TsUniqueList<T> Create<T>(params T[] elements) => new(TsList.Create(elements));
    public static TsUniqueList<T> Create<T>() => new(TsList.Create<T>());
    public static TsUniqueList<T> CreateRange<T>(IEnumerable<T> enumerable) => new(TsList.CreateRange(enumerable));
}

public record TsUniqueList<T> : IEnumerable<T>
{
    public TsUniqueList(IEnumerable<T> enumerable)
    {
        var observedElements = new HashSet<T>();
        var uniqueElements = new List<T>();
        foreach (var element in enumerable)
        {
            if (!observedElements.Contains(element))
            {
                uniqueElements.Add(element);
                observedElements.Add(element);
            }
        }

        _list = new TsList<T>(uniqueElements);
    }

    public int Count => _list.Count;

    public TsUniqueList<T> Add(T element) => new(_list.Add(element));
    public TsUniqueList<T> Add(params T[] elements) => AddRange(elements);
    public TsUniqueList<T> AddRange(IEnumerable<T> elements) => new(_list.AddRange(elements));
    public TAcc Aggregate<TAcc>(TAcc seed, Func<TAcc, T, TAcc> aggregator) => _list.Aggregate(seed, aggregator);
    public bool Contains(T element) => _list.Contains(element);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    public TsUniqueList<TResult> Select<TResult>(Func<T, TResult> selector) => new(_list.Select(selector));

    public TsDict<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector) where TKey : notnull
        => ToDictionary(keySelector, value => value);

    public TsDict<TKey, TValue> ToDictionary<TKey, TValue>(
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector) where TKey : notnull
        => TsDict<TKey, TValue>.Create(this, keySelector, valueSelector);

    public TsUniqueList<T> Where(Func<T, bool> predicate) => new(_list.Where(predicate));

    #region Private

    private readonly TsList<T> _list;
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    #endregion

    #region Equality Members

    public virtual bool Equals(TsUniqueList<T>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _list.Equals(other._list);
    }

    public override int GetHashCode() => _list.GetHashCode();

    #endregion
}
