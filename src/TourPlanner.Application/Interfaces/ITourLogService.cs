using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Interfaces;

public interface ITourLogService
{
    Task<IReadOnlyList<TourLog>> GetByTourAsync(Guid tourId, CancellationToken ct = default);
    Task<TourLog> CreateAsync(Guid tourId, DateTime date, string? notes, int rating, CancellationToken ct = default);
    Task UpdateAsync(TourLog log, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
