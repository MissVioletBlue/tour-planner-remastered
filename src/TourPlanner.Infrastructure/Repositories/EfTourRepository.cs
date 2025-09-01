using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;

namespace TourPlanner.Infrastructure.Repositories;

public sealed class EfTourRepository : ITourRepository
{
    private readonly AppDbContext _db;
    public EfTourRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await _db.Tours.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Tour>> SearchAsync(SearchRequest r, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var page = Math.Max(1, r.Page);
        var size = Math.Clamp(r.PageSize, 1, 200);
        var text = r.Text?.Trim();

        IQueryable<Tour> q = _db.Tours.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var lower = text.ToLower();
            q = q.Where(t =>
                t.Name.ToLower().Contains(lower) ||
                (t.Description != null && t.Description.ToLower().Contains(lower)) ||
                t.From.ToLower().Contains(lower) ||
                t.To.ToLower().Contains(lower) ||
                t.TransportType.ToLower().Contains(lower));
        }

        if (r.MinRating is int minR)
            q = q.Where(t => _db.TourLogs.Any(l => l.TourId == t.Id && l.Rating >= minR));

        if (r.DateFrom is DateTime df)
            q = q.Where(t => _db.TourLogs.Any(l => l.TourId == t.Id && l.Date >= df));

        if (r.DateTo is DateTime dt)
            q = q.Where(t => _db.TourLogs.Any(l => l.TourId == t.Id && l.Date <= dt));

        q = r.SortBy switch
        {
            "DistanceKm" => r.Desc ? q.OrderByDescending(x => x.DistanceKm) : q.OrderBy(x => x.DistanceKm),
            _            => r.Desc ? q.OrderByDescending(x => x.Name)       : q.OrderBy(x => x.Name)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<Tour> CreateAsync(Tour tour, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _db.Tours.Add(tour);
        await _db.SaveChangesAsync(ct);
        _db.Entry(tour).State = EntityState.Detached;
        return tour;
    }

    public async Task UpdateAsync(Tour tour, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var tracked = _db.Tours.Local.FirstOrDefault(e => e.Id == tour.Id);
        if (tracked is not null)
        {
            _db.Entry(tracked).State = EntityState.Detached;
        }

        _db.Tours.Update(tour);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await _db.Tours.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task<IReadOnlyList<TourSummaryDto>> GetSummariesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await _db.Tours
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TourSummaryDto(
                t.Id,
                t.Name,
                t.DistanceKm,
                _db.TourLogs.Count(l => l.TourId == t.Id),
                _db.TourLogs.Where(l => l.TourId == t.Id)
                    .Select(l => (double?)l.Rating)
                    .Average(),
                null))
            .ToListAsync(ct);
    }
}
