using Microsoft.Extensions.Logging;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Services;

public sealed class TourService : ITourService
{
    private readonly ITourRepository _repo;
    private readonly ILogger<TourService> _log;
    public TourService(ITourRepository repo, ILogger<TourService> log)
    {
        _repo = repo;
        _log = log;
    }

    public Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);

    public Task<PagedResult<Tour>> SearchAsync(SearchRequest req, CancellationToken ct = default)
        => _repo.SearchAsync(req with { Page = Math.Max(1, req.Page), PageSize = Math.Clamp(req.PageSize, 1, 200) }, ct);

    public async Task<Tour> CreateAsync(string name, string? description, double distanceKm, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationFailedException("Name required");
        if (distanceKm < 0) throw new ValidationFailedException("Distance must be non-negative");
        var tour = new Tour(Guid.NewGuid(), name.Trim(), description?.Trim(), distanceKm);
        _log.LogInformation("Creating tour {Name}", tour.Name);
        return await _repo.CreateAsync(tour, ct);
    }

    public async Task UpdateAsync(Tour tour, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tour.Name)) throw new ValidationFailedException("Name required");
        _log.LogInformation("Updating tour {Id}", tour.Id);
        await _repo.UpdateAsync(tour with { Name = tour.Name.Trim() }, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _log.LogInformation("Deleting tour {Id}", id);
        var affected = await _repo.DeleteAsync(id, ct);
        if (affected == 0) throw new NotFoundException(nameof(Tour), id);
    }

    public Task<IReadOnlyList<TourSummaryDto>> GetSummariesAsync(CancellationToken ct = default)
        => _repo.GetSummariesAsync(ct);
}

