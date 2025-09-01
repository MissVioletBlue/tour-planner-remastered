using Microsoft.Extensions.Logging.Abstractions;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Infrastructure.Services;
using TourPlanner.Tests.Utils;
using Xunit;
using System.IO;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Tests.Infrastructure;

public class MapImageTests
{
    [Fact]
    public async Task Stub_Map_Service_Writes_File()
    {
        var svc = new StubMapService();
        var res = await svc.GetRouteAsync("A","B");
        Assert.True(File.Exists(res.ImagePath));
    }

    [Fact]
    public async Task TourService_Create_Sets_Image_Path()
    {
        ITourRepository repo = new InMemoryTourRepository();
        ITourLogRepository logRepo = new InMemoryTourLogRepository();
        IMapService map = new StubMapService();
        var svc = new TourPlanner.Application.Services.TourService(repo, logRepo, map, NullLogger<TourPlanner.Application.Services.TourService>.Instance);
        var t = await svc.CreateAsync("Img", null, "A", "B", "car");
        Assert.False(string.IsNullOrWhiteSpace(t.RouteImagePath));
        Assert.True(File.Exists(t.RouteImagePath));
    }
}
