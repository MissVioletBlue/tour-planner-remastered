using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TourPlanner.Application.Interfaces;
using TourPlanner.Application.Services;
using TourPlanner.Infrastructure.Repositories;
using Xunit;

namespace TourPlanner.Tests.Services;

public class TourLogServiceUpvoteTests
{
    [Fact]
    public async Task Upvote_Increments_Count_And_Sorts()
    {
        ITourLogRepository repo = new InMemoryTourLogRepository();
        var service = new TourLogService(repo, NullLogger<TourLogService>.Instance);
        var tourId = Guid.NewGuid();
        var l1 = await service.CreateAsync(tourId, DateTime.Today, "A", 1, 0, TimeSpan.Zero, 3);
        var l2 = await service.CreateAsync(tourId, DateTime.Today, "B", 1, 0, TimeSpan.Zero, 3);
        await service.UpvoteAsync(l2);
        var logs = await service.GetByTourAsync(tourId);
        Assert.Equal(l2.Id, logs[0].Id);
        Assert.Equal(1, logs[0].Votes);
        Assert.Equal(0, logs[1].Votes);
    }
}
