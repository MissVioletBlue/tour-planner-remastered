using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Contracts;

public sealed record TourExportDto(
    Guid Id,
    string Name,
    string? Description,
    double DistanceKm,
    IReadOnlyList<TourLog> Logs);
