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
        throw new InvalidOperationException(
            "OpenRouteService API key is not configured. Set 'OpenRouteService:ApiKey' in appsettings.json to enable routing.");
    }
}
