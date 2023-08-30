using System;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.Shape;

namespace TypeSharper.Sample;

[TsIntersection<Rect, Square, Circle>]
public partial record ShapeIntersection;

public static class IntersectionSample
{
    public static ShapeIntersection Circle(Circle circle) => new(circle);

    public static void Ctor() => PrintShapeType(new ShapeIntersection(new Circle(EShapeType.Circle, 1, 2, 10)));
    public static void ImplicitCast() => PrintShapeType(new Circle(EShapeType.Circle, 1, 2, 10));

    public static void PrintShapeType(ShapeIntersection shapeIntersection)
        => Console.WriteLine(
            $"Shape {shapeIntersection.Type} at position ({shapeIntersection.X}, {shapeIntersection.Y})");

    public static ShapeIntersection Rect(Rect rect) => new(rect);
    public static ShapeIntersection Square(Square square) => new(square);

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
