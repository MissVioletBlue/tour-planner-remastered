using System.IO;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;

namespace TourPlanner.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly AppDbContext _db;
    public ReportService(AppDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> BuildTourReportAsync(Guid tourId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var tour = await _db.Tours.FirstOrDefaultAsync(t => t.Id == tourId, ct)
                   ?? throw new NotFoundException(nameof(Tour), tourId);
        var logs = await _db.TourLogs.Where(l => l.TourId == tourId).OrderByDescending(l => l.Date).ToListAsync(ct);

        using var stream = new MemoryStream();
        Document.Create(d =>
        {
            d.Page(p =>
            {
                p.Content().Column(col =>
                {
                    col.Item().Text($"Tour: {tour.Name}").FontSize(20).Bold();
                    col.Item().Text($"Distance: {tour.DistanceKm} km");
                    col.Item().Text($"Logs: {logs.Count}");
                    foreach (var l in logs)
                        col.Item().Text($"{l.Date:yyyy-MM-dd}  ★{l.Rating}  {l.Notes}");
                });
            });
        }).GeneratePdf(stream);
        return stream.ToArray();
    }
}
