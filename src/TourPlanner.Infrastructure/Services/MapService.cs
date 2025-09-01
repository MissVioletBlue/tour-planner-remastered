using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

/// <summary>
/// Maps addresses using the OpenRouteService HTTP API. An API key must be
/// configured; otherwise a route cannot be resolved.
/// </summary>
public sealed class MapService : IMapService
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;

    public MapService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = configuration["OpenRouteService:ApiKey"];
    }

    public async Task<RouteResult> GetRouteAsync(string from, string to, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("OpenRouteService API key is not configured.");

        var s = await GeocodeAsync(from, ct);
        var e = await GeocodeAsync(to, ct);

        var req = new
        {
            coordinates = new[]
            {
                new[] { s.Lng, s.Lat },
                new[] { e.Lng, e.Lat }
            }
        };

        using var msg = new HttpRequestMessage(HttpMethod.Post, "v2/directions/driving-car")
        {
            Content = JsonContent.Create(req)
        };
        msg.Headers.Add("Authorization", _apiKey);

        using var resp = await _http.SendAsync(msg, ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        var feat = json.GetProperty("features")[0];
        var summary = feat.GetProperty("properties").GetProperty("summary");
        var distanceKm = summary.GetProperty("distance").GetDouble() / 1000.0;
        var duration = TimeSpan.FromSeconds(summary.GetProperty("duration").GetDouble());
        var coords = feat.GetProperty("geometry").GetProperty("coordinates");
        var path = new List<(double Lat, double Lng)>();
        foreach (var c in coords.EnumerateArray())
        {
            path.Add((c[1].GetDouble(), c[0].GetDouble()));
        }

        return new RouteResult(distanceKm, duration, path);
    }

    private async Task<(double Lat, double Lng)> GeocodeAsync(string text, CancellationToken ct)
    {
        var url = $"geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(text)}&size=1";
        var json = await _http.GetFromJsonAsync<JsonElement>(url, ct);
        var coords = json.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates");
        return (coords[1].GetDouble(), coords[0].GetDouble());
    }
}

