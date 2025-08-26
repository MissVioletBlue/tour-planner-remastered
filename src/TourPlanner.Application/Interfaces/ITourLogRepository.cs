using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Interfaces;

public interface ITourLogRepository
{
    Task<IReadOnlyList<TourLog>> GetByTourAsync(Guid tourId, CancellationToken ct = default);
    Task<TourLog> CreateAsync(TourLog log, CancellationToken ct = default);
    Task UpdateAsync(TourLog log, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
