using System.Text;
using Microsoft.EntityFrameworkCore;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Services;
using TourPlanner.Tests.Utils;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class ReportServiceSummaryTests
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
    public async Task Summary_Report_Returns_Pdf()
    {
        using var db = NewDb();
        var tours = new EfTourRepository(db);
        var logs  = new EfTourLogRepository(db);
        var t = await tours.CreateAsync(TestHelper.NewTour("S1", 5));
        await logs.CreateAsync(TestHelper.NewLog(t.Id, "ok", 4));
        var svc = new ReportService(tours, logs);
        var bytes = await svc.BuildSummaryReportAsync();
        Assert.True(bytes.Length > 0);
        var ascii = Encoding.ASCII.GetString(bytes);
        Assert.StartsWith("%PDF", ascii);
        Assert.Contains("%%EOF", ascii);
    }
}
