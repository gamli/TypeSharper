using System;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.Shape;

namespace TypeSharper.Sample;

[TypeSharperTaggedUnion<Rect, Square, Circle>("FourCorners", "FourCornersAndEqualEdges", "RoundThing")]
public abstract partial record ShapeTaggedUnion;

public static class TaggedUnionSample
{
    public static double Area(ShapeTaggedUnion shape)
        => shape.Match(
            rect => rect.Width * rect.Height,
            square => square.Size * square.Size,
            circle => Math.PI * circle.Radius * circle.Radius);

    public static ShapeTaggedUnion CreateFourCorners(Rect rect) => new ShapeTaggedUnion.FourCorners(rect);

    public static ShapeTaggedUnion CreateFourCornersAndEqualEdges(Square square)
        => new ShapeTaggedUnion.FourCornersAndEqualEdges(square);

    public static ShapeTaggedUnion CreateRoundThing(Circle circle) => new ShapeTaggedUnion.RoundThing(circle);
}
