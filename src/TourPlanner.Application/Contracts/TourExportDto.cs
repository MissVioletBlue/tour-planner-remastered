using System;
using System.Collections.Generic;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Contracts;

public sealed record TourExportDto(
    Guid Id,
    string Name,
    string? Description,
    string From,
    string To,
    string TransportType,
    double DistanceKm,
    TimeSpan EstimatedTime,
    IReadOnlyList<(double Lat, double Lng)> Route,
    IReadOnlyList<TourLog> Logs);
