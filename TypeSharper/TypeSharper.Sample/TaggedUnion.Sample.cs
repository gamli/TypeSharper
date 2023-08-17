using System;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.Shape;

namespace TypeSharper.Sample;

[TypeSharperTaggedUnion<Rect, Square, Circle>("FourCorners", "FourCornersAndEqualEdges", "RoundThing")]
public abstract partial record ShapeTaggedUnion;

public static class TaggedUnionSample
{
    public static ShapeTaggedUnion CreateFourCorners(Rect rect) => ShapeTaggedUnion.CreateFourCorners(rect);

    public static ShapeTaggedUnion CreateFourCornersAndEqualEdges(Square square)
        => ShapeTaggedUnion.CreateFourCornersAndEqualEdges(square);

    public static ShapeTaggedUnion CreateRoundThing(Circle circle) => ShapeTaggedUnion.CreateRoundThing(circle);

    public static double Area(ShapeTaggedUnion shape)
        => shape.Match(
            rect => rect.Width * rect.Height,
            square => square.Size * square.Size,
            circle => Math.PI * circle.Radius * circle.Radius);
}
