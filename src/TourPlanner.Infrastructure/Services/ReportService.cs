using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using System.Linq;
using System.Collections.Generic;

namespace TourPlanner.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly ITourRepository _tours;
    private readonly ITourLogRepository _logs;

    public ReportService(ITourRepository tours, ITourLogRepository logs)
    {
        _tours = tours;
        _logs = logs;
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableCaching = false;
    }

    public async Task<byte[]> BuildTourReportAsync(Guid tourId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var tour = (await _tours.GetAllAsync(ct)).FirstOrDefault(t => t.Id == tourId)
                   ?? throw new NotFoundException(nameof(Tour), tourId);
        var logs = (await _logs.GetByTourAsync(tourId, ct)).OrderByDescending(l => l.Date).ToList();

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

        return stream.ToArray();
    }

    public async Task<byte[]> BuildSummaryReportAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var tours = await _tours.GetAllAsync(ct);
        var data = new List<(string Name, double? AvgDist, TimeSpan? AvgTime, double? AvgRating)>();
        foreach (var t in tours)
        {
            var logs = await _logs.GetByTourAsync(t.Id, ct);
            double? avgDist = logs.Count > 0 ? logs.Average(l => (double?)l.TotalDistance) : null;
            TimeSpan? avgTime = logs.Count > 0 ? TimeSpan.FromTicks((long)logs.Average(l => (double?)l.TotalTime.Ticks)) : null;
            double? avgRating = logs.Count > 0 ? logs.Average(l => (double?)l.Rating) : null;
            data.Add((t.Name, avgDist, avgTime, avgRating));
        }

        using var stream = new MemoryStream();
        Document.Create(d =>
        {
            d.Page(p =>
            {
                p.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Tour").Bold();
                        h.Cell().Text("Avg Dist").Bold();
                        h.Cell().Text("Avg Time").Bold();
                        h.Cell().Text("Avg Rating").Bold();
                    });

                    foreach (var item in data)
                    {
                        table.Cell().Text(item.Name);
                        table.Cell().Text(item.AvgDist?.ToString("F1") ?? "-");
                        table.Cell().Text(item.AvgTime?.ToString() ?? "-");
                        table.Cell().Text(item.AvgRating?.ToString("F1") ?? "-");
                    }
                });
            });
        }).GeneratePdf(stream);

        return stream.ToArray();
    }
}
