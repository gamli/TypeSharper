using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TypeSharper;

public static class TsSortedSet
{
    public static TsSortedSet<T> Create<T>(params T[] elements) => new(elements);
    public static TsSortedSet<T> Create<T>(IEnumerable<T> elements) => new(elements);
}

public record TsSortedSet<T> : TsSet<T>
{
    public TsSortedSet(params T[] elements) : this((IEnumerable<T>)elements) { }
    public TsSortedSet(IEnumerable<T> elements) : this(ImmutableSortedSet.CreateRange(elements)) { }
    public TsSortedSet(ImmutableSortedSet<T> immutableSortedSet) : base(immutableSortedSet) { }
    public static implicit operator TsSortedSet<T>(ImmutableSortedSet<T> immutableSortedSet) => new(immutableSortedSet);
}

public static class TsHashSet
{
    public static TsHashSet<T> Create<T>(params T[] elements) => new(elements);
    public static TsHashSet<T> Create<T>(IEnumerable<T> elements) => new(elements);
}

public record TsHashSet<T> : TsSet<T>
{
    public TsHashSet(params T[] elements) : this((IEnumerable<T>)elements) { }
    public TsHashSet(IEnumerable<T> elements) : this(ImmutableHashSet.CreateRange(elements)) { }
    public TsHashSet(ImmutableHashSet<T> immutableHashSet) : base(immutableHashSet) { }
    public static implicit operator TsHashSet<T>(ImmutableHashSet<T> immutableHashSet) => new(immutableHashSet);
}

public abstract record TsSet<T> : IImmutableSet<T>
{
    public int Count => _immutableSet.Count;
    public IImmutableSet<T> Add(T value) => _immutableSet.Add(value);

    public IImmutableSet<T> Clear() => _immutableSet.Clear();
    public bool Contains(T value) => _immutableSet.Contains(value);
    public IImmutableSet<T> Except(IEnumerable<T> other) => _immutableSet.Except(other);
    public IEnumerator<T> GetEnumerator() => _immutableSet.GetEnumerator();

    public IImmutableSet<T> Intersect(IEnumerable<T> other) => _immutableSet.Intersect(other);
    public bool IsProperSubsetOf(IEnumerable<T> other) => _immutableSet.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => _immutableSet.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => _immutableSet.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => _immutableSet.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => _immutableSet.Overlaps(other);
    public IImmutableSet<T> Remove(T value) => _immutableSet.Remove(value);
    public bool SetEquals(IEnumerable<T> other) => _immutableSet.SetEquals(other);
    public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) => _immutableSet.SymmetricExcept(other);

    public bool TryGetValue(T equalValue, out T actualValue) => _immutableSet.TryGetValue(equalValue, out actualValue);

    public IImmutableSet<T> Union(IEnumerable<T> other) => _immutableSet.Union(other);

    #region Protected

    protected TsSet(IImmutableSet<T> immutableSet) => _immutableSet = immutableSet;

    #endregion

    #region Private

    private readonly IImmutableSet<T> _immutableSet;

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_immutableSet).GetEnumerator();

    #endregion
}
