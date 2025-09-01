using TourPlanner.Application.Contracts;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Interfaces;

public interface ITourService
{
    Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResult<Tour>> SearchAsync(SearchRequest req, CancellationToken ct = default);
    Task<Tour> CreateAsync(string name, string? description, string from, string to, string transportType, CancellationToken ct = default);
    Task<Tour> UpdateAsync(Tour tour, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TourSummaryDto>> GetSummariesAsync(CancellationToken ct = default);
}