using System;
using TypeSharper.Attributes;

namespace TypeSharper.Sample;

public record MappingSource(string Name, bool IsSample, int Count);

[TsOmitAttribute<MappingSource>(Mappings = new[] { "Name", "bool", "Count", "bool" })]
public partial record MappingTarget;

public static class MappingSample
{
    public static MappingTarget Create() => new(false, false, false);

    public static void Print(MappingTarget mappingTarget)
        => Console.WriteLine(mappingTarget is { Name: true, IsSample: true, Count: true });
}
