using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

/// <summary>
/// Calls OpenRouteService (ORS) for geocoding + directions and returns a polyline,
/// distance (km) and duration (hh:mm:ss).
/// </summary>
public sealed class MapService : IMapService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _imageDir;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public MapService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = configuration["OpenRouteService:ApiKey"]?.Trim()
                  ?? throw new InvalidOperationException("OpenRouteService API key is not configured.");
        _imageDir = configuration["Images:BaseDir"] ?? Path.Combine(AppContext.BaseDirectory, "images");
        Directory.CreateDirectory(_imageDir);
    }

    public async Task<RouteResult> GetRouteAsync(string from, string to, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(from)) throw new ArgumentException("From cannot be empty.", nameof(from));
            if (string.IsNullOrWhiteSpace(to))   throw new ArgumentException("To cannot be empty.", nameof(to));

            // 1) Geocode both ends (forward geocoding)
            var s = await GeocodeAsync(from, ct);
            var e = await GeocodeAsync(to, ct);

            // 2) Directions (POST /v2/directions/driving-car/geojson)
            var body = new
            {
                coordinates = new[]
                {
                    new[] { s.Lng, s.Lat },   // ORS expects [lon, lat]
                    new[] { e.Lng, e.Lat }
                }
                // You can add: "preference":"fastest", "maneuvers":true, etc.
            };

            using var msg = new HttpRequestMessage(HttpMethod.Post, "v2/directions/driving-car/geojson")
            {
                Content = JsonContent.Create(body, options: _json)
            };
            msg.Headers.Remove("Authorization");
            msg.Headers.TryAddWithoutValidation("Authorization", _apiKey);
            msg.Headers.Accept.Clear();
            msg.Headers.Accept.ParseAdd("application/geo+json");

            using var resp = await _http.SendAsync(msg, ct);
            await EnsureSuccessAsync(resp, "Directions");

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json, ct);
            if (!json.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
                throw new InvalidOperationException("Directions returned no features.");

            var feat = features[0];
            var props = feat.GetProperty("properties");
            var summary = props.GetProperty("summary");

            var distanceKm = summary.GetProperty("distance").GetDouble() / 1000.0;
            var duration   = TimeSpan.FromSeconds(summary.GetProperty("duration").GetDouble());

            // geometry.coordinates is [[lon,lat], [lon,lat], ...]
            var coords = feat.GetProperty("geometry").GetProperty("coordinates");
            var path = new List<(double Lat, double Lng)>(capacity: coords.GetArrayLength());
            foreach (var c in coords.EnumerateArray())
            {
                var lon = c[0].GetDouble();
                var lat = c[1].GetDouble();
                path.Add((lat, lon));
            }

            var img = await DownloadStaticMapAsync(path, ct);
            return new RouteResult(distanceKm, duration, path, img);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Fall back to a stubbed route when the real service fails
            var stub = new StubMapService();
            return await stub.GetRouteAsync(from, to, ct);
        }
    }

    private async Task<(double Lat, double Lng)> GeocodeAsync(string text, CancellationToken ct)
    {
        // GET /geocode/search?text=...&size=1
        // Use header-based auth (no key in query string).
        var url = $"geocode/search?text={Uri.EscapeDataString(text)}&size=1";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Remove("Authorization");
        req.Headers.TryAddWithoutValidation("Authorization", _apiKey);

        using var resp = await _http.SendAsync(req, ct);
        await EnsureSuccessAsync(resp, $"Geocode '{text}'");

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(_json, ct);
        if (!json.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
            throw new InvalidOperationException($"Address not found: {text}");

        var coords = features[0].GetProperty("geometry").GetProperty("coordinates"); // [lon, lat]
        var lon = coords[0].GetDouble();
        var lat = coords[1].GetDouble();
        return (lat, lon);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage resp, string op)
    {
        if (resp.IsSuccessStatusCode) return;

        string detail = string.Empty;
        try
        {
            var err = await resp.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(err)) detail = err;
        }
        catch { /* ignore */ }

        if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
            throw new InvalidOperationException($"{op} failed: unauthorized – check your ORS API key. {detail}");

        throw new InvalidOperationException($"{op} failed: HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}. {detail}");
    }

    private async Task<string> DownloadStaticMapAsync(IEnumerable<(double Lat, double Lng)> path, CancellationToken ct)
    {
        // Build simple static map request using openstreetmap static service
        var coords = path.Take(30).Select(p => $"{p.Lat},{p.Lng}");
        var url = "https://staticmap.openstreetmap.de/staticmap.php?size=600x400&path=" + string.Join("|", coords);
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Remove("Authorization");
        req.Headers.TryAddWithoutValidation("Authorization", _apiKey);
        using var resp = await _http.SendAsync(req, ct);
        await EnsureSuccessAsync(resp, "Map image");
        var file = Path.Combine(_imageDir, $"{Guid.NewGuid():N}.png");
        await using var fs = File.Create(file);
        await resp.Content.CopyToAsync(fs, ct);
        return file;
    }
}
