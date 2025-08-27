namespace TourPlanner.Application.Contracts;

public sealed record SearchRequest(
    string? Text,
    int? MinRating,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? SortBy,   // "Name","Date","Rating","DistanceKm"
    bool Desc,
    int Page,         // 1-based
    int PageSize
);