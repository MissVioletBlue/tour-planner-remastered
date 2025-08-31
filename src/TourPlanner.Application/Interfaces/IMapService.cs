using TourPlanner.Application.Contracts;

namespace TourPlanner.Application.Interfaces;

public interface IMapService
{
    Task<RouteResult> CalculateRouteAsync(string from, string to);
}

