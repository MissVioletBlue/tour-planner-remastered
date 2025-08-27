namespace TourPlanner.Domain.Entities;

public record Tour(
    Guid Id,
    string Name,
    string? Description,
    double DistanceKm
);
