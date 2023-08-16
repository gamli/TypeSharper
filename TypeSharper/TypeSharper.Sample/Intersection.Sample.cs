using System;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.Shape;

namespace TypeSharper.Sample;

[TypeSharperIntersection<Rect, Square, Circle>]
public partial record AnyShape;

public static class IntersectionSample
{
    public static AnyShape Circle(Circle circle) => new(circle);

    public static void ImplicitCast() => PrintShapeType(new Circle(EShapeType.Circle, 1, 2, 10));

    public static void PrintShapeType(AnyShape shape)
        => Console.WriteLine($"Shape {shape.Type} at position ({shape.X}, {shape.Y})");

    public static AnyShape Rect(Rect rect) => new(rect);
    public static AnyShape Square(Square square) => new(square);

    public static void TypeInference(EShapeType shapeType)
        => PrintShapeType(
            shapeType switch
            {
                EShapeType.Rect   => new Rect(EShapeType.Rect, 0, 0, 1, 1),
                EShapeType.Square => new Square(EShapeType.Square, 0, 0, 1),
                EShapeType.Circle => new Circle(EShapeType.Circle, 0, 0, 1),
                _                 => throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, null),
            });
}
