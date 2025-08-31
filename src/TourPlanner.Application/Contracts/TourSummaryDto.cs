namespace TourPlanner.Application.Contracts;

public sealed record TourSummaryDto(
    Guid Id,
    string Name,
    double DistanceKm,
    int LogsCount,
    double? AverageRating);
