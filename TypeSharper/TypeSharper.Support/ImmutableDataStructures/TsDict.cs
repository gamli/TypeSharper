using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TypeSharper;

public record TsDict<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    public TsDict() : this(ImmutableDictionary<TKey, TValue>.Empty) { }

    public TsDict(IEnumerable<KeyValuePair<TKey, TValue>> kvs) => _dictionary = ImmutableDictionary.CreateRange(kvs);

    public int Count => _dictionary.Count;

    public TValue this[TKey key] => _dictionary[key];

    public IEnumerable<TKey> Keys => _dictionary.Keys;

    public IEnumerable<TValue> Values => _dictionary.Values;

    public static TsDict<TKey, TValue> Create<T>(
        IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector)
        => new(
            ImmutableDictionary.CreateRange(
                enumerable.Select(item => new KeyValuePair<TKey, TValue>(keySelector(item), valueSelector(item)))));

    public TsDict<TKey, TValue> Add(TKey key, TValue value) => new(_dictionary.Add(key, value));
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);


    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    public TsDict<TKey, TValue> Remove(TKey key) => new(_dictionary.Remove(key));
    public TsDict<TKey, TValue> Set(TKey key, TValue value) => new(_dictionary.SetItem(key, value));
    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);
    public TsDict<TKey, TValue> Update(TKey key, Func<TValue, TValue> updateValue) => Set(key, updateValue(this[key]));

    #region Private

    private readonly ImmutableDictionary<TKey, TValue> _dictionary;
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dictionary).GetEnumerator();

    #endregion

    #region Equality Members

    public virtual bool Equals(TsDict<TKey, TValue>? other)
        => !ReferenceEquals(null, other)
           && (ReferenceEquals(this, other)
               || _dictionary.SequenceEqual(other._dictionary));

    public override int GetHashCode() => _dictionary.Aggregate(19, (hash, kv) => hash * 31 + kv.GetHashCode());

    #endregion
}
