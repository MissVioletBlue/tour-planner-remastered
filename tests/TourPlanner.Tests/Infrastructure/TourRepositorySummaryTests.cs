using System.Linq;
using Microsoft.EntityFrameworkCore;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class TourRepositorySummaryTests
{
    private static AppDbContext NewDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;
        var db = new AppDbContext(opts);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task Summaries_Handle_Logs_And_NoLogs()
    {
        using var db = NewDb();
        var repo = new EfTourRepository(db);
        var logRepo = new EfTourLogRepository(db);

        var t1 = await repo.CreateAsync(TestHelper.NewTour("Alps", 10));
        var t2 = await repo.CreateAsync(TestHelper.NewTour("Beach", 5));
        await logRepo.CreateAsync(TestHelper.NewLog(t1.Id, "Nice", 4));
        await logRepo.CreateAsync(TestHelper.NewLog(t1.Id, "Great", 2));

        var sums = await repo.GetSummariesAsync();
        Assert.Equal(2, sums.Count);
        var s1 = sums.First(s => s.Id == t1.Id);
        Assert.Equal(2, s1.Popularity);
        Assert.Equal(3, s1.AverageRating);
        var s2 = sums.First(s => s.Id == t2.Id);
        Assert.Equal(0, s2.Popularity);
        Assert.Null(s2.AverageRating);
    }
}
