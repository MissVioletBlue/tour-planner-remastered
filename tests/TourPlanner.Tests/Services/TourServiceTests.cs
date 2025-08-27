using TourPlanner.Application.Interfaces;
using TourPlanner.Infrastructure.Repositories;
using Xunit;

namespace TourPlanner.Tests.Services;

public class TourServiceTests
{
    [Fact]
    public async Task Create_Adds_Tour()
    {
        ITourRepository repo = new InMemoryTourRepository();
        var svc = new TourPlanner.Application.Services.TourService(repo);

        var created = await svc.CreateAsync("Test Tour", null, 0);
        var all = await svc.GetAllAsync();

        Assert.Contains(all, t => t.Id == created.Id && t.Name == "Test Tour");
    }
}
