namespace TourPlanner.Application.Contracts;

public sealed record TourSummaryDto(
    Guid Id,
    string Name,
    double DistanceKm,
    int Popularity,
    double? AverageRating,
    double? ChildFriendliness);
