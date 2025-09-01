using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class TourRepositoryLogSearchTests
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
    public async Task Search_Finds_By_Log_Comment()
    {
        using var db = NewDb();
        var tours = new EfTourRepository(db);
        var logs  = new EfTourLogRepository(db);
        var t = await tours.CreateAsync(TestHelper.NewTour("T1", 5));
        await logs.CreateAsync(TestHelper.NewLog(t.Id, "fantastic", 4));
        var res = await tours.SearchAsync(new SearchRequest("fantastic", null, null, null, "Name", false, 1, 10));
        Assert.Single(res.Items);
    }

    [Fact]
    public async Task Search_Finds_By_Log_Rating()
    {
        using var db = NewDb();
        var tours = new EfTourRepository(db);
        var logs  = new EfTourLogRepository(db);
        var t = await tours.CreateAsync(TestHelper.NewTour("T2", 5));
        await logs.CreateAsync(TestHelper.NewLog(t.Id, "", 5));
        var res = await tours.SearchAsync(new SearchRequest("5", null, null, null, "Name", false, 1, 10));
        Assert.Single(res.Items);
    }
}
