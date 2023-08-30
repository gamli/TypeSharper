using System;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.Shape;

namespace TypeSharper.Sample;

[TsTaggedUnion<Rect, Square, Circle>("FourCorners", "FourCornersAndEqualEdges", "RoundThing")]
public abstract partial record ShapeTaggedUnion;

public static class TaggedUnionSample
{
    public static double Area(ShapeTaggedUnion shape)
        => shape.Map(
            rect => rect.Width * rect.Height,
            square => square.Size * square.Size,
            circle => Math.PI * circle.Radius * circle.Radius);

    public static ShapeTaggedUnion CreateFourCorners(Rect rect) => new ShapeTaggedUnion.FourCorners(rect);

    public static ShapeTaggedUnion CreateFourCornersAndEqualEdges(Square square)
        => new ShapeTaggedUnion.FourCornersAndEqualEdges(square);

    public static ShapeTaggedUnion CreateRoundThing(Circle circle) => new ShapeTaggedUnion.RoundThing(circle);

    public static string AreaIfRoundThing(ShapeTaggedUnion shape)
        => shape
           .IfRoundThing(circle => circle.Radius * circle.Radius * Math.PI)
           .IfSome(area => area + 9)
           .Map(area => $"Area is {area}", () => "Not a Circle");
}

[TsTaggedUnion<bool>("Some", "None")]
public abstract partial record MyMaybe;
