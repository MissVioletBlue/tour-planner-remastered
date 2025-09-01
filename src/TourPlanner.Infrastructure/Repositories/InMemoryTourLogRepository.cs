using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Repositories;

public sealed class InMemoryTourLogRepository : ITourLogRepository
{
    private readonly List<TourLog> _logs = new();

    public Task<IReadOnlyList<TourLog>> GetByTourAsync(Guid tourId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var list = _logs
            .Where(x => x.TourId == tourId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Rating)
            .ToList();
        return Task.FromResult((IReadOnlyList<TourLog>)list);
    }

    public Task<TourLog> CreateAsync(TourLog log, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _logs.Add(log);
        return Task.FromResult(log);
    }

    public Task UpdateAsync(TourLog log, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var idx = _logs.FindIndex(l => l.Id == log.Id);
        if (idx >= 0) _logs[idx] = log;
        return Task.CompletedTask;
    }

    public Task<int> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var removed = _logs.RemoveAll(l => l.Id == id);
        return Task.FromResult(removed);
    }
}

