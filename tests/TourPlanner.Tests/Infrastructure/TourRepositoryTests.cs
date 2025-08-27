using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class TourRepositoryTests
{
    private static AppDbContext NewSqliteDb()
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
    public async Task Create_And_GetAll_Works()
    {
        using var db = NewSqliteDb();
        var repo = new EfTourRepository(db);

        await repo.CreateAsync(new(Guid.NewGuid(), "Alps Trail", null, 12.3));
        await repo.CreateAsync(new(Guid.NewGuid(), "Bavarian Walk", null, 7.0));

        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count);
        Assert.Equal(new[] { "Alps Trail", "Bavarian Walk" }, all.Select(x => x.Name).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task Search_Paginates_And_Filters()
    {
        using var db = NewSqliteDb();
        var repo = new EfTourRepository(db);

        for (int i = 1; i <= 25; i++)
            await repo.CreateAsync(new(Guid.NewGuid(), $"Tour {i:D2}", null, i));

        var page1 = await repo.SearchAsync(new SearchRequest("Tour", null, null, null, "Name", false, 1, 10));
        var page3 = await repo.SearchAsync(new SearchRequest("Tour", null, null, null, "Name", false, 3, 10));

        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(5,  page3.Items.Count);
        Assert.Equal(25, page1.TotalCount);
    }

    [Fact]
    public async Task Update_And_Delete_Works()
    {
        using var db = NewSqliteDb();
        var repo = new EfTourRepository(db);

        var t = await repo.CreateAsync(new(Guid.NewGuid(), "Old", null, 1));
        await repo.UpdateAsync(t with { Name = "New" });

        var all = await repo.GetAllAsync();
        Assert.Single(all);
        Assert.Equal("New", all[0].Name);

        await repo.DeleteAsync(t.Id);
        var empty = await repo.GetAllAsync();
        Assert.Empty(empty);
    }
}
