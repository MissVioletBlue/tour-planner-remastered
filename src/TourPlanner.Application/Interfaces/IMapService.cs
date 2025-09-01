using TourPlanner.Application.Contracts;
using System.Threading;

namespace TourPlanner.Application.Interfaces;

public interface IMapService
{
    Task<RouteResult> GetRouteAsync(string from, string to, CancellationToken ct = default);
}

