using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

public sealed class MapService : IMapService
{
    public Task<RouteResult> CalculateRouteAsync(string from, string to)
    {
        var result = new RouteResult(42, TimeSpan.FromHours(1));
        return Task.FromResult(result);
    }
}

