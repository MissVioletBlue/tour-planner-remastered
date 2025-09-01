using System.Collections.Generic;

namespace TourPlanner.Application.Contracts;

public sealed record RouteResult(double DistanceKm, TimeSpan EstimatedTime, IReadOnlyList<(double Lat, double Lng)> Path);

