using Microsoft.EntityFrameworkCore;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Persistence;

namespace TourPlanner.Infrastructure.Repositories;

public sealed class EfTourLogRepository : ITourLogRepository
{
    private readonly AppDbContext _db;
    public EfTourLogRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<TourLog>> GetByTourAsync(Guid tourId, CancellationToken ct = default)
        => await _db.TourLogs.AsNoTracking()
            .Where(x => x.TourId == tourId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Rating)
            .ToListAsync(ct);

    public async Task<TourLog> CreateAsync(TourLog log, CancellationToken ct = default)
    {
        _db.TourLogs.Add(log);
        await _db.SaveChangesAsync(ct);
        _db.Entry(log).State = EntityState.Detached;
        return log;
    }

    public async Task UpdateAsync(TourLog log, CancellationToken ct = default)
    {
        _db.TourLogs.Update(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await _db.TourLogs.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
}
