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

    public Task<Tour> DeleteAsync(Tour tour, CancellationToken ct = default)
    {
        if (_tours.Remove(tour))
        {
            return Task.FromResult<Tour?>(tour)!;
        }

        return Task.FromResult<Tour?>(null)!;
    }

    /// Tour bearbeiten, ursprüngliche tour und neue tour erhalten, dann einfach alte tour = neue tour?

    public Task<Tour?> EditAsync(Tour tour, CancellationToken ct = default)
    {
        var index = _tours.FindIndex(t => t.Id == tour.Id);
        if (index == -1)
        {
            return Task.FromResult<Tour?>(null)!; // Not found
        }

        _tours[index] = tour; // replace old record with new record
        return Task.FromResult<Tour?>(_tours[index])!;
    }

}
