using System;
using TypeSharper.Attributes;
using TypeSharper.Sample.SampleBaseTypes.Shape;

namespace TypeSharper.Sample;

public record FirstFrom(int A, int B);

public record SecondFrom(int C, int D);

[TsProduct<FirstFrom, SecondFrom>]
public partial record ProductTo;

public static class ProductSample
{
    public static ProductTo CreateFrom() => new(new FirstFrom(1, 2), new SecondFrom(3, 4));
    public static ProductTo Create() => new(1, 2, 3, 4);
    public static void Use(ProductTo p) { Console.Write("{0}, {1}, {2}, {3}", p.A, p.B, p.C, p.D); }
}
