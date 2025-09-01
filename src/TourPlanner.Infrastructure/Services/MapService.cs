using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using System.Collections.Generic;

namespace TourPlanner.Infrastructure.Services;

public sealed class MapService : IMapService
{
    public Task<RouteResult> GetRouteAsync(string from, string to, System.Threading.CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var path = new List<(double Lat, double Lng)>
        {
            (48.2082, 16.3738),
            (48.2100, 16.3600),
            (48.2150, 16.3400)
        };
        var result = new RouteResult(42, TimeSpan.FromHours(1), path);
        return Task.FromResult(result);
    }
}

