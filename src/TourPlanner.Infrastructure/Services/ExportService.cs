using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;

namespace TourPlanner.Infrastructure.Services;

public sealed class ExportService : IExportService
{
    private readonly AppDbContext _db;
    public ExportService(AppDbContext db) => _db = db;

    public async Task<byte[]> ExportToursAsync(IEnumerable<Guid> ids, string format, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var tours = await _db.Tours.Where(t => ids.Contains(t.Id)).ToListAsync(ct);
        var logs  = await _db.TourLogs.Where(l => ids.Contains(l.TourId)).ToListAsync(ct);

        var items = tours.Select(t => new TourExportDto(
            t.Id,
            t.Name,
            t.Description,
            t.From,
            t.To,
            t.TransportType,
            t.DistanceKm,
            t.EstimatedTime,
            t.Route,
            t.RouteImagePath,
            logs.Where(l => l.TourId == t.Id).ToList()
        )).ToList();

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return JsonSerializer.SerializeToUtf8Bytes(items, new JsonSerializerOptions { WriteIndented = true });

        if (string.Equals(format, "gpx", StringComparison.OrdinalIgnoreCase))
        {
            var t = tours.Single();
            var sbGpx = new StringBuilder();
            sbGpx.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sbGpx.AppendLine("<gpx version=\"1.1\" creator=\"TourPlanner\">");
            sbGpx.AppendLine($"  <trk><name>{Escape(t.Name)}</name><trkseg>");
            foreach (var p in t.Route)
                sbGpx.AppendLine($"    <trkpt lat=\"{p.Lat}\" lon=\"{p.Lng}\" />");
            sbGpx.AppendLine("  </trkseg></trk></gpx>");
            return Encoding.UTF8.GetBytes(sbGpx.ToString());
        }

        var sb = new StringBuilder().AppendLine("Id,Name,Description,DistanceKm");
        foreach (var t in tours)
            sb.AppendLine($"{t.Id},{Escape(t.Name)},{Escape(t.Description)},{t.DistanceKm}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<int> ImportToursAsync(byte[] content, string format, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        {
            var items = JsonSerializer.Deserialize<List<TourExportDto>>(content) ?? new();
            foreach (var dto in items)
            {
                var exists = await _db.Tours.AnyAsync(x => x.Id == dto.Id, ct);
                if (!exists) _db.Tours.Add(new Tour(
                    dto.Id,
                    dto.Name,
                    dto.Description,
                    dto.From,
                    dto.To,
                    dto.TransportType,
                    dto.DistanceKm,
                    dto.EstimatedTime,
                    dto.Route ?? new List<(double,double)>(),
                    dto.RouteImagePath ?? string.Empty
                ));
                foreach (var l in dto.Logs)
                    if (!await _db.TourLogs.AnyAsync(x => x.Id == l.Id, ct))
                        _db.TourLogs.Add(l);
            }
            return await _db.SaveChangesAsync(ct);
        }
        throw new NotSupportedException("Only json supported for import.");
    }

    private static string Escape(string? s) => (s ?? "").Replace(",", "\\,");
}
