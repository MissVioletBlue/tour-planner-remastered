using TourPlanner.Domain.Entities;
using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Infrastructure.Repositories;

public sealed class InMemoryTourRepository : ITourRepository
{
    private readonly List<Tour> _tours = new();

    public Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<Tour>)_tours);

    public Task<Tour> CreateAsync(Tour tour, CancellationToken ct = default)
    {
        _tours.Add(tour);
        return Task.FromResult(tour);
    }
}
