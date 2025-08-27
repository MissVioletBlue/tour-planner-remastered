using Microsoft.EntityFrameworkCore;
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
        => await _db.Tours.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);

    public async Task<PagedResult<Tour>> SearchAsync(SearchRequest r, CancellationToken ct = default)
    {
        var page = Math.Max(1, r.Page);
        var size = Math.Clamp(r.PageSize, 1, 200);
        var pattern = r.Text?.Trim();

        IQueryable<Tour> q = _db.Tours.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(pattern))
        {
            var p = pattern.ToLower();
            q = q.Where(x => x.Name.ToLower().Contains(p));
        }

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
        _db.Tours.Add(tour);
        await _db.SaveChangesAsync(ct);
        return tour;
    }

    public async Task UpdateAsync(Tour tour, CancellationToken ct = default)
    {
        _db.Tours.Update(tour);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _db.Tours.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
    }
}
