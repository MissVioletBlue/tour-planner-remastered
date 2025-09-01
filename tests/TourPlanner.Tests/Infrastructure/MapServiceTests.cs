using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using TourPlanner.Infrastructure.Services;
using Xunit;

namespace TourPlanner.Tests.Infrastructure;

public class MapServiceTests
{
    [Fact]
    public async Task GetRoute_ParsesApiResponses()
    {
        var handler = new FakeHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.openrouteservice.org/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["OpenRouteService:ApiKey"] = "test" })
            .Build();

        var svc = new MapService(client, cfg);
        var result = await svc.GetRouteAsync("A", "B");

        Assert.Equal(1, result.DistanceKm);
        Assert.Equal(TimeSpan.FromSeconds(600), result.EstimatedTime);
        Assert.Equal(2, result.Path.Count);
        Assert.Equal((0.0, 0.0), result.Path[0]);
        Assert.Equal((1.0, 1.0), result.Path[1]);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.True(request.Headers.TryGetValues("Authorization", out var auth));
            Assert.Equal("test", auth.Single());

            string json;
            if (request.RequestUri!.AbsolutePath.Contains("geocode"))
            {
                var textParam = request.RequestUri.Query.Split('&').First(p => p.Contains("text=")).Split('=')[1];
                var text = Uri.UnescapeDataString(textParam);
                json = text == "A"
                    ? "{\"features\":[{\"geometry\":{\"coordinates\":[0,0]}}]}"
                    : "{\"features\":[{\"geometry\":{\"coordinates\":[1,1]}}]}";
            }
            else
            {
                json = "{\"features\":[{\"properties\":{\"summary\":{\"distance\":1000,\"duration\":600}},\"geometry\":{\"coordinates\":[[0,0],[1,1]]}}]}";
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}

