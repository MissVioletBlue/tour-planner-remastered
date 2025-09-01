using System;
using System.Threading;
using System.Threading.Tasks;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

public sealed class StubMapService : IMapService
{
    public Task<RouteResult> GetRouteAsync(string from, string to, CancellationToken ct = default)
    {
        return Task.FromResult(new RouteResult(0, TimeSpan.Zero, Array.Empty<(double Lat, double Lng)>()));
    }
}
