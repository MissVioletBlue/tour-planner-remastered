using Microsoft.Extensions.Logging.Abstractions;
using TourPlanner.Application.Interfaces;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Infrastructure.Services;
using Xunit;

namespace TourPlanner.Tests.Services;

public class TourServiceTests
{
    [Fact]
    public async Task Create_Adds_Tour()
    {
        ITourRepository repo = new InMemoryTourRepository();
        ITourLogRepository logRepo = new InMemoryTourLogRepository();
        IMapService map = new TourPlanner.Infrastructure.Services.StubMapService();
        var svc = new TourPlanner.Application.Services.TourService(repo, logRepo, map, NullLogger<TourPlanner.Application.Services.TourService>.Instance);

        var created = await svc.CreateAsync("Test Tour", null, "A", "B", "car");
        var all = await svc.GetAllAsync();

        Assert.Contains(all, t => t.Id == created.Id && t.Name == "Test Tour");
    }
}
