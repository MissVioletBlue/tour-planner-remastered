using System.Text;
using Microsoft.EntityFrameworkCore;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Infrastructure.Services;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class ExportServiceGpxTests
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
    public async Task Export_Gpx_Contains_Trkpt()
    {
        using var db = NewDb();
        var repo = new EfTourRepository(db);
        var logRepo = new EfTourLogRepository(db);
        var svc = new ExportService(db);
        var t = await repo.CreateAsync(TestHelper.NewTour("Gpx", 1));
        var bytes = await svc.ExportToursAsync(new[] { t.Id }, "gpx");
        var text = Encoding.UTF8.GetString(bytes);
        Assert.Contains("<gpx", text);
        Assert.Contains("<trkpt", text);
    }
}
