using System;
using System.Collections.Generic;

namespace TourPlanner.Domain.Entities;

public record Tour(
    Guid Id,
    string Name,
    string? Description,
    string From,
    string To,
    string TransportType,
    double DistanceKm,
    TimeSpan EstimatedTime,
    List<(double Lat, double Lng)> Route,
    string RouteImagePath
);
