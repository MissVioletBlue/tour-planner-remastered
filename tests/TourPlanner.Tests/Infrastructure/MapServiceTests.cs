using TourPlanner.Infrastructure.Services;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class MapServiceTests
{
    [Fact]
    public async Task GetRoute_Returns_FixedValues()
    {
        var svc = new MapService();
        var result = await svc.GetRouteAsync("A", "B");
        Assert.Equal(42, result.DistanceKm);
        Assert.Equal(TimeSpan.FromHours(1), result.EstimatedTime);
        Assert.True(result.Path.Count > 0);
    }
}

