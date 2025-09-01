using System.Text;
using Microsoft.EntityFrameworkCore;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Services;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class ReportServiceTests
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
    public async Task Report_Contains_Tour_Name()
    {
        using var db = NewDb();
        var repo = new TourPlanner.Infrastructure.Repositories.EfTourRepository(db);
        var logRepo = new TourPlanner.Infrastructure.Repositories.EfTourLogRepository(db);
        var svc = new ReportService(db);

        var t = await repo.CreateAsync(TestHelper.NewTour("PdfTest", 1));
        await logRepo.CreateAsync(TestHelper.NewLog(t.Id, "note", 4));

        var bytes = await svc.BuildTourReportAsync(t.Id);
        Assert.True(bytes.Length > 0);
        var text = Encoding.UTF8.GetString(bytes);
        Assert.Contains("PdfTest", text);
    }
}
