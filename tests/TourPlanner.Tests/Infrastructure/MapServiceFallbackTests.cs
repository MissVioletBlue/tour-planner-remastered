using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Configuration;
using TourPlanner.Infrastructure.Services;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class MapServiceFallbackTests
{
    [Fact]
    public async Task FallsBackToStubWhenRequestFails()
    {
        var handler = new FailHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.openrouteservice.org/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["OpenRouteService:ApiKey"] = "test" })
            .Build();

        var svc = new MapService(client, cfg);
        var res = await svc.GetRouteAsync("A", "B");

        Assert.Equal(1.0, res.DistanceKm);
        Assert.True(System.IO.File.Exists(res.ImagePath));
    }

    private sealed class FailHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
    }
}
