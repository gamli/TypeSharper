using Xunit;

namespace TypeSharper.Tests.NetApi;

public class ImmutableTest
{
    [Fact]
    public void Immutable_list_equality()
    {
        var rgbList1 = TsList.Create("Red", "Green", "Blue");
        var rgbList2 = TsList.Create("Red", "Green", "Blue");
        Assert.True(rgbList1 == rgbList2);
    }
}
