using Microsoft.Extensions.Logging.Abstractions;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Infrastructure.Services;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Services;

public class TourServiceLogSearchTests
{
    [Fact]
    public async Task Search_Finds_By_Log_Comment()
    {
        ITourRepository repo = new InMemoryTourRepository();
        ITourLogRepository logRepo = new InMemoryTourLogRepository();
        IMapService map = new StubMapService();
        var svc = new TourPlanner.Application.Services.TourService(repo, logRepo, map, NullLogger<TourPlanner.Application.Services.TourService>.Instance);
        var t = await svc.CreateAsync("Trip", null, "A", "B", "car");
        await logRepo.CreateAsync(TestHelper.NewLog(t.Id, "awesome ride", 5));
        var res = await svc.SearchAsync(new SearchRequest("awesome", null, null, null, "Name", false, 1, 10));
        Assert.Single(res.Items);
    }

    [Fact]
    public async Task Search_Finds_By_Log_Rating()
    {
        ITourRepository repo = new InMemoryTourRepository();
        ITourLogRepository logRepo = new InMemoryTourLogRepository();
        IMapService map = new StubMapService();
        var svc = new TourPlanner.Application.Services.TourService(repo, logRepo, map, NullLogger<TourPlanner.Application.Services.TourService>.Instance);
        var t = await svc.CreateAsync("Trip2", null, "A", "B", "car");
        await logRepo.CreateAsync(TestHelper.NewLog(t.Id, "", 5));
        var res = await svc.SearchAsync(new SearchRequest("5", null, null, null, "Name", false, 1, 10));
        Assert.Single(res.Items);
    }

    [Fact]
    public async Task Search_Finds_By_Log_Difficulty()
    {
        ITourRepository repo = new InMemoryTourRepository();
        ITourLogRepository logRepo = new InMemoryTourLogRepository();
        IMapService map = new StubMapService();
        var svc = new TourPlanner.Application.Services.TourService(repo, logRepo, map, NullLogger<TourPlanner.Application.Services.TourService>.Instance);
        var t = await svc.CreateAsync("Trip3", null, "A", "B", "car");
        await logRepo.CreateAsync(TestHelper.NewLog(t.Id, "", 3) with { Difficulty = 7 });
        var res = await svc.SearchAsync(new SearchRequest("7", null, null, null, "Name", false, 1, 10));
        Assert.Single(res.Items);
    }
}
