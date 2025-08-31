using Microsoft.EntityFrameworkCore;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Services;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class ExportServiceTests
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
    public async Task Export_Import_Roundtrip_Works()
    {
        using var db = NewDb();
        var repo = new TourPlanner.Infrastructure.Repositories.EfTourRepository(db);
        var logRepo = new TourPlanner.Infrastructure.Repositories.EfTourLogRepository(db);
        var svc = new ExportService(db);

        var t = await repo.CreateAsync(new(Guid.NewGuid(), "Round", null, 1));
        await logRepo.CreateAsync(new(Guid.NewGuid(), t.Id, DateTime.Today, "ok", 3));

        var json = await svc.ExportToursAsync(new[] { t.Id }, "json");

        db.TourLogs.RemoveRange(db.TourLogs);
        db.Tours.RemoveRange(db.Tours);
        await db.SaveChangesAsync();

        var imported = await svc.ImportToursAsync(json, "json");
        Assert.Equal(2, imported); // one tour + one log
        Assert.Single(db.Tours);
        Assert.Single(db.TourLogs);
    }
}
