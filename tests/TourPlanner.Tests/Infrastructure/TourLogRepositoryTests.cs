using Microsoft.EntityFrameworkCore;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class TourLogRepositoryTests
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
    public async Task Create_Get_Update_Delete_Works()
    {
        using var db = NewDb();
        var tours = new EfTourRepository(db);
        var logs  = new EfTourLogRepository(db);

        var t = await tours.CreateAsync(TestHelper.NewTour("Vienna Ring", 6.2));
        var l = await logs.CreateAsync(TestHelper.NewLog(t.Id, "Nice", 4));

        var byTour = await logs.GetByTourAsync(t.Id);
        Assert.Single(byTour);
        Assert.Equal(4, byTour[0].Rating);

        await logs.UpdateAsync(l with { Rating = 5 });
        var upd = await logs.GetByTourAsync(t.Id);
        Assert.Equal(5, upd[0].Rating);

        var affected = await logs.DeleteAsync(l.Id);
        Assert.Equal(1, affected);
        var empty = await logs.GetByTourAsync(t.Id);
        Assert.Empty(empty);
    }
}
