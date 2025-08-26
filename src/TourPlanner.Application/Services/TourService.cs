using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Services;

public sealed class TourService : ITourService
{
    private readonly ITourRepository _repo;
    public TourService(ITourRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);

    public Task<PagedResult<Tour>> SearchAsync(SearchRequest req, CancellationToken ct = default)
        => _repo.SearchAsync(req with { Page = Math.Max(1, req.Page), PageSize = Math.Clamp(req.PageSize, 1, 200) }, ct);

    public Task<Tour> CreateAsync(string name, string? description, double distanceKm, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required");
        if (distanceKm < 0) throw new ArgumentOutOfRangeException(nameof(distanceKm));
        var tour = new Tour(Guid.NewGuid(), name.Trim(), description?.Trim(), distanceKm);
        return _repo.CreateAsync(tour, ct);
    }

    public Task UpdateAsync(Tour tour, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tour.Name)) throw new ArgumentException("Name required");
        return _repo.UpdateAsync(tour with { Name = tour.Name.Trim() }, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => _repo.DeleteAsync(id, ct);
}

