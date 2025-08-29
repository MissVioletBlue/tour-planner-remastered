using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Services;

public sealed class TourLogService : ITourLogService
{
    private readonly ITourLogRepository _repo;
    public TourLogService(ITourLogRepository repo) => _repo = repo;

    public Task<IReadOnlyList<TourLog>> GetByTourAsync(Guid tourId, CancellationToken ct = default)
        => _repo.GetByTourAsync(tourId, ct);

    public Task<TourLog> CreateAsync(Guid tourId, DateTime date, string? notes, int rating, CancellationToken ct = default)
    {
        if (rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(rating));
        var log = new TourLog(Guid.NewGuid(), tourId, date, notes?.Trim(), rating);
        return _repo.CreateAsync(log, ct);
    }

    public Task UpdateAsync(TourLog log, CancellationToken ct = default)
    {
        if (log.Rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(log.Rating));
        return _repo.UpdateAsync(log with { Notes = log.Notes?.Trim() }, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _repo.DeleteAsync(id, ct);
}
