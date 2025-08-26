using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Interfaces;

public interface ITourRepository
{
    Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default);
    Task<Tour> CreateAsync(Tour tour, CancellationToken ct = default);
    Task<Tour> DeleteAsync(Tour tour, CancellationToken ct = default);
    Task<Tour> EditAsync(Tour tour, CancellationToken ct = default);


    }
