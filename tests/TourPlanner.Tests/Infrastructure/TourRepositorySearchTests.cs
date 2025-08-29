using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class TourRepositorySearchTests
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
    public async Task Search_By_Text_MinRating_DateRange_Works()
    {
        using var db = NewDb();
        var tours = new EfTourRepository(db);
        var logs  = new EfTourLogRepository(db);

        var t1 = await tours.CreateAsync(new(Guid.NewGuid(), "Alps Trail", null, 12));
        var t2 = await tours.CreateAsync(new(Guid.NewGuid(), "City Walk", null, 3));
        await logs.CreateAsync(new(Guid.NewGuid(), t1.Id, new DateTime(2025, 1, 10), "ok", 3));
        await logs.CreateAsync(new(Guid.NewGuid(), t1.Id, new DateTime(2025, 2, 10), "great", 5));
        await logs.CreateAsync(new(Guid.NewGuid(), t2.Id, new DateTime(2025, 1, 15), "meh", 2));

        var resText = await tours.SearchAsync(new SearchRequest("Alps", null, null, null, "Name", false, 1, 10));
        Assert.Single(resText.Items);
        Assert.Equal("Alps Trail", resText.Items[0].Name);

        var resRating = await tours.SearchAsync(new SearchRequest(null, 4, null, null, "Name", false, 1, 10));
        Assert.Single(resRating.Items);
        Assert.Equal("Alps Trail", resRating.Items[0].Name);

        var resDate = await tours.SearchAsync(new SearchRequest(null, null, new DateTime(2025,2,1), new DateTime(2025,2,28), "Name", false, 1, 10));
        Assert.Single(resDate.Items);
        Assert.Equal("Alps Trail", resDate.Items[0].Name);
    }
}
