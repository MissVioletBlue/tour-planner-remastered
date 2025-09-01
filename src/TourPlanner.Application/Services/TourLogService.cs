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

    public async Task<TourLog> CreateAsync(Guid tourId, DateTime date, string? comment, int difficulty, double totalDistance, TimeSpan totalTime, int rating, CancellationToken ct = default)
    {
        if (rating is < 1 or > 5) throw new ValidationFailedException("Rating must be between 1 and 5");
        if (difficulty is < 1 or > 5) throw new ValidationFailedException("Difficulty must be between 1 and 5");
        if (totalDistance < 0) throw new ValidationFailedException("Distance must be non-negative");
        if (totalTime < TimeSpan.Zero) throw new ValidationFailedException("Time must be non-negative");
        var utcDate = date.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => date
        };
        var log = new TourLog(Guid.NewGuid(), tourId, utcDate, comment?.Trim(), difficulty, totalDistance, totalTime, rating, 0);
        _log.LogInformation("Creating log for tour {TourId}", tourId);
        return await _repo.CreateAsync(log, ct);
    }

    public async Task UpdateAsync(TourLog log, CancellationToken ct = default)
    {
        if (log.Rating is < 1 or > 5) throw new ValidationFailedException("Rating must be between 1 and 5");
        if (log.Difficulty is < 1 or > 5) throw new ValidationFailedException("Difficulty must be between 1 and 5");
        if (log.TotalDistance < 0) throw new ValidationFailedException("Distance must be non-negative");
        if (log.TotalTime < TimeSpan.Zero) throw new ValidationFailedException("Time must be non-negative");
        _log.LogInformation("Updating log {Id}", log.Id);
        var utcDate = log.Date.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(log.Date, DateTimeKind.Utc),
            DateTimeKind.Local => log.Date.ToUniversalTime(),
            _ => log.Date
        };
        await _repo.UpdateAsync(log with { Comment = log.Comment?.Trim(), Date = utcDate }, ct);
    }

    public async Task<TourLog> UpvoteAsync(TourLog log, CancellationToken ct = default)
    {
        _log.LogInformation("Upvoting log {Id}", log.Id);
        var updated = log with { Votes = log.Votes + 1 };
        await _repo.UpdateAsync(updated, ct);
        return updated;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _log.LogInformation("Deleting log {Id}", id);
        var affected = await _repo.DeleteAsync(id, ct);
        if (affected == 0) throw new NotFoundException(nameof(TourLog), id);
    }
}
