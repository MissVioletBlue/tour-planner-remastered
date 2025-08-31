using TourPlanner.Application.Contracts;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Interfaces;

public interface ITourService
{
    Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Tour>> SearchAsync(SearchRequest req, CancellationToken ct = default);
    Task<Tour> CreateAsync(string name, string? description, double distanceKm, CancellationToken ct = default);
    Task UpdateAsync(Tour tour, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TourSummaryDto>> GetSummariesAsync(CancellationToken ct = default);
}