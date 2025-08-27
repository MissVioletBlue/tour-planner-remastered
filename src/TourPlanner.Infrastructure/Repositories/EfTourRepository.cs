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
        IQueryable<Tour> q = _db.Tours.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(r.Text))
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{r.Text}%"));

        if (r.SortBy is "Date" or "Rating") { /* optional für aggregierte Sortierung */ }
        else if (r.SortBy == "DistanceKm") q = r.Desc ? q.OrderByDescending(x => x.DistanceKm) : q.OrderBy(x => x.DistanceKm);
        else q = r.Desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((r.Page - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new(items, r.Page, r.PageSize, total);
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
