using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Application.Services;

public sealed class TourService : ITourService
{
    private readonly ITourRepository _repo;

    public TourService(ITourRepository repo)
    {
        _repo = repo;
    }

    public Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<Tour> CreateAsync(string name, string? description, double distanceKm, CancellationToken ct = default)
    {
        var tour = new Tour(Guid.NewGuid(), name.Trim(), description?.Trim(), distanceKm);
        return _repo.CreateAsync(tour, ct);
    }
}
