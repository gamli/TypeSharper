using System;

namespace TypeSharper.Model;

public abstract partial record TsType(TsType.TypeInfo Info) : IComparable<TsType>
{
    public string CsFile(TsModel model)
        => $$"""
            {{Info.Ns.CsFileScoped()}}

            {{Cs(model)}}
            """;

    public abstract string Cs(TsModel model);

    public Maybe<T> IfDuck<T>(Func<Duck, T> handleDuck) => this is Duck duck ? handleDuck(duck) : Maybe<T>.NONE;

    public Maybe<T> IfIntersection<T>(Func<Intersection, T> handleIntersection)
        => this is Intersection intersection ? handleIntersection(intersection) : Maybe<T>.NONE;

    public Maybe<T> IfNative<T>(Func<TsType, T> handleNative)
        => this is Native native ? handleNative(native) : Maybe<T>.NONE;

    public Maybe<T> IfOmitted<T>(Func<Omitted, T> handleOmitted)
        => this is Omitted omitted ? handleOmitted(omitted) : Maybe<T>.NONE;

    public Maybe<T> IfPicked<T>(Func<Picked, T> handlePicked)
        => this is Picked picked ? handlePicked(picked) : Maybe<T>.NONE;

    public Maybe<T> IfPropertyDuck<T>(Func<Duck, T> handlePropertyDuck)
        => this is PropertyDuck propertyDuck ? handlePropertyDuck(propertyDuck) : Maybe<T>.NONE;

    public Maybe<T> IfTaggedUnion<T>(Func<TaggedUnion, T> handleTaggedUnion)
        => this is TaggedUnion taggedUnion ? handleTaggedUnion(taggedUnion) : Maybe<T>.NONE;

    public bool IsTopLevel() => Info.ContainingType.Map(_ => false, () => true);

    public T Map<T>(
        Func<Picked, T> handlePicked,
        Func<Omitted, T> handleOmitted,
        Func<Intersection, T> handleIntersection,
        Func<TaggedUnion, T> handleTaggedUnion,
        Func<Native, T> handleNative)
        => this switch
        {
            Picked picked             => handlePicked(picked),
            Omitted omitted           => handleOmitted(omitted),
            Intersection intersection => handleIntersection(intersection),
            TaggedUnion taggedUnion   => handleTaggedUnion(taggedUnion),
            Native native             => handleNative(native),
            _                         => throw new ArgumentOutOfRangeException(),
        };

    public T Map<T>(
        Func<PropertySelection, T> handlePropertySelection,
        Func<Intersection, T> handleIntersection,
        Func<TaggedUnion, T> handleTaggedUnion,
        Func<Native, T> handleNative)
        => this switch
        {
            PropertySelection propertySelection => handlePropertySelection(propertySelection),
            Intersection intersection           => handleIntersection(intersection),
            TaggedUnion taggedUnion             => handleTaggedUnion(taggedUnion),
            Native native                       => handleNative(native),
            _                                   => throw new ArgumentOutOfRangeException(),
        };

    public T Map<T>(
        Func<PropertyDuck, T> handlePropertyDuck,
        Func<TaggedUnion, T> handleTaggedUnion,
        Func<Native, T> handleNative)
        => this switch
        {
            PropertyDuck propertyDuck => handlePropertyDuck(propertyDuck),
            TaggedUnion taggedUnion   => handleTaggedUnion(taggedUnion),
            Native native             => handleNative(native),
            _                         => throw new ArgumentOutOfRangeException(),
        };

    public T MatchDuckOrNative<T>(Func<Duck, T> handleDuck, Func<TsType, T> handleNative)
        => this switch
        {
            Duck duck     => handleDuck(duck),
            Native native => handleNative(native),
            _             => throw new ArgumentOutOfRangeException(),
        };

    public TsTypeRef Ref() => Info.Ref();
    public int CompareTo(TsType other) => Info.CompareTo(other.Info);
}
