using Microsoft.Extensions.Logging;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Services;

public sealed class TourLogService : ITourLogService
{
    private readonly ITourLogRepository _repo;
    private readonly ILogger<TourLogService> _log;
    public TourLogService(ITourLogRepository repo, ILogger<TourLogService> log)
    {
        _repo = repo;
        _log = log;
    }

    public Task<IReadOnlyList<TourLog>> GetByTourAsync(Guid tourId, CancellationToken ct = default)
        => _repo.GetByTourAsync(tourId, ct);

    public async Task<TourLog> CreateAsync(Guid tourId, DateTime date, string? notes, int rating, CancellationToken ct = default)
    {
        if (rating is < 1 or > 5) throw new ValidationFailedException("Rating must be between 1 and 5");
        var log = new TourLog(Guid.NewGuid(), tourId, date, notes?.Trim(), rating);
        _log.LogInformation("Creating log for tour {TourId}", tourId);
        return await _repo.CreateAsync(log, ct);
    }

    public async Task UpdateAsync(TourLog log, CancellationToken ct = default)
    {
        if (log.Rating is < 1 or > 5) throw new ValidationFailedException("Rating must be between 1 and 5");
        _log.LogInformation("Updating log {Id}", log.Id);
        await _repo.UpdateAsync(log with { Notes = log.Notes?.Trim() }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _log.LogInformation("Deleting log {Id}", id);
        var affected = await _repo.DeleteAsync(id, ct);
        if (affected == 0) throw new NotFoundException(nameof(TourLog), id);
    }
}
