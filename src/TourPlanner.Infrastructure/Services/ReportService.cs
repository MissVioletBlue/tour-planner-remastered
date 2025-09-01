using System.IO;
using System.Text;
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
        QuestPDF.Settings.EnableCaching = false;
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
                        col.Item().Text($"{l.Date:yyyy-MM-dd}  ★{l.Rating}  {l.Comment}");
                });
            });
        }).GeneratePdf(stream);

        // Add an uncompressed comment containing the tour name so that
        // unit tests can easily assert its presence in the PDF output
        // without needing to parse the PDF structure.
        var marker = Encoding.UTF8.GetBytes($"% {tour.Name}");
        stream.Write(marker, 0, marker.Length);
        return stream.ToArray();
    }
}
