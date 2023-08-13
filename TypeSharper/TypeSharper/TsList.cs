using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TypeSharper;

public static class TsList
{
    public static TsList<T> Create<T>(IEnumerable<T> enumerable) => new(enumerable);
    public static TsList<T> Create<T>(params T[] elements) => new(elements);
    public static TsList<T> Create<T>() => new();
}

public record TsList<T> : IEnumerable<T>
{
    public TsList(IEnumerable<T> enumerable) => _list = ImmutableList.CreateRange(enumerable);
    public TsList() => _list = ImmutableList.Create<T>();
    public TsList(params T[] elements) : this((IEnumerable<T>)elements) { }
    public static TsList<T> Empty => new();
    public int Count => _list.Count;

    public TsList<T> Add(T element) => new(_list.Add(element));
    public TsList<T> Add(params T[] elements) => AddRange(elements);
    public TsList<T> AddRange(IEnumerable<T> elements) => new(_list.AddRange(elements));
    public TAcc Aggregate<TAcc>(TAcc seed, Func<TAcc, T, TAcc> aggregator) => _list.Aggregate(seed, aggregator);
    public bool Contains(T element) => _list.Contains(element);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    public TsList<TResult> Select<TResult>(Func<T, TResult> selector) => new(_list.Select(selector));

    public TsDict<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector) where TKey : notnull
        => ToDictionary(keySelector, value => value);

    public TsDict<TKey, TValue> ToDictionary<TKey, TValue>(
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector) where TKey : notnull
        => TsDict<TKey, TValue>.Create(this, keySelector, valueSelector);

    public TsList<T> Where(Func<T, bool> predicate) => new(_list.Where(predicate));

    #region Private

    private readonly ImmutableList<T> _list;
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    #endregion

    #region Equality Members

    public virtual bool Equals(TsList<T>? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _list.SequenceEqual(other._list);
    }

    public override int GetHashCode() => _list.Aggregate(19, (hash, item) => hash * 31 + (item?.GetHashCode() ?? 0));

    #endregion
}
