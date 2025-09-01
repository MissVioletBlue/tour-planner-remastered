using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

public sealed class StubMapService : IMapService
{
    public Task<RouteResult> GetRouteAsync(string from, string to, CancellationToken ct = default)
    {
        // Return a fixed demo route of 1 km taking 00:15:00
        var path = new List<(double Lat, double Lng)>
        {
            (48.0, 16.0),
            (48.01, 16.01)
        };
        var dir = Path.Combine(AppContext.BaseDirectory, "images");
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "stub.png");
        if (!File.Exists(file))
            File.WriteAllBytes(file, new byte[] { 0 });
        return Task.FromResult(new RouteResult(1.0, TimeSpan.FromMinutes(15), path, file));
    }
}
