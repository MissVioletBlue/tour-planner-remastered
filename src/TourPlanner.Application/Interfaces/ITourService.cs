using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Interfaces;

public interface ITourService
{
    Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default);
    Task<Tour> CreateAsync(string name, string? description, string From, string To, string TransportType, double distanceKm, CancellationToken ct = default);
}
