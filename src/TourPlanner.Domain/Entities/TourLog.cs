using System;

namespace TourPlanner.Domain.Entities;

public record TourLog(
    Guid Id,
    Guid TourId,
    DateTime Date,
    string? Comment,
    int Difficulty,
    double TotalDistance,
    TimeSpan TotalTime,
    int Rating  // 1..5
);
