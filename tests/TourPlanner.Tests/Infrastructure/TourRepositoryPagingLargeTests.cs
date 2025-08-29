using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class TourRepositoryPagingLargeTests
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
    public async Task Paging_Remains_Stable_With_Many_Rows()
    {
        using var db = NewDb();
        var repo = new EfTourRepository(db);

        for (int i = 1; i <= 1000; i++)
            await repo.CreateAsync(new(Guid.NewGuid(), $"Tour {i:D4}", null, i % 100));

        var p1 = await repo.SearchAsync(new SearchRequest("Tour", null, null, null, "Name", false, 1, 50));
        var p2 = await repo.SearchAsync(new SearchRequest("Tour", null, null, null, "Name", false, 2, 50));

        Assert.Equal(50, p1.Items.Count);
        Assert.Equal(50, p2.Items.Count);
        Assert.NotEqual(p1.Items[0].Id, p2.Items[0].Id);
    }
}
