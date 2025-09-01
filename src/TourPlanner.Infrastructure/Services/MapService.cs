using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

/// <summary>
/// Maps addresses using the OpenRouteService HTTP API. If an API key is not
/// configured the service falls back to returning a straight line between the
/// supplied points which are interpreted as comma separated coordinates
/// ("lat,lng").
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
        {
            var start = ParseCoordinate(from);
            var end = ParseCoordinate(to);
            var path = new List<(double Lat, double Lng)> { start, end };
            var distanceKm = Haversine(start, end);
            return new RouteResult(distanceKm, TimeSpan.Zero, path);
        }

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

    private static (double Lat, double Lng) ParseCoordinate(string value)
    {
        var parts = value.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 2) throw new ArgumentException("Coordinate format is 'lat,lng'", nameof(value));
        return (double.Parse(parts[0], CultureInfo.InvariantCulture), double.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    private static double Haversine((double Lat, double Lng) a, (double Lat, double Lng) b)
    {
        const double R = 6371; // earth radius km
        double dLat = Deg2Rad(b.Lat - a.Lat);
        double dLng = Deg2Rad(b.Lng - a.Lng);
        double sinLat = Math.Sin(dLat / 2);
        double sinLng = Math.Sin(dLng / 2);
        double h = sinLat * sinLat + Math.Cos(Deg2Rad(a.Lat)) * Math.Cos(Deg2Rad(b.Lat)) * sinLng * sinLng;
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }

    private static double Deg2Rad(double d) => d * Math.PI / 180.0;
}

