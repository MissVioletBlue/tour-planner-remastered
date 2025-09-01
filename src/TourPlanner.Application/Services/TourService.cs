using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Application.Services;

public sealed class TourService : ITourService
{
    private readonly ITourRepository _repo;
    private readonly ITourLogRepository _logRepo;
    private readonly IMapService _mapService;
    private readonly ILogger<TourService> _log;

    public TourService(ITourRepository repo, ITourLogRepository logRepo, IMapService mapService, ILogger<TourService> log)
    {
        _repo = repo;
        _logRepo = logRepo;
        _mapService = mapService;
        _log = log;
    }

    public Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);

    public async Task<PagedResult<Tour>> SearchAsync(SearchRequest req, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var tours = await _repo.GetAllAsync(ct);
        var list = new List<Tour>();
        foreach (var t in tours)
        {
            var logs = await _logRepo.GetByTourAsync(t.Id, ct);
            var popularity = logs.Count;
            var child = ComputeChildFriendliness(logs);
            if (string.IsNullOrWhiteSpace(req.Text) || Contains(t, logs, popularity, child, req.Text))
                list.Add(t);
        }

        IEnumerable<Tour> q = list;
        q = req.SortBy switch
        {
            "DistanceKm" => req.Desc ? q.OrderByDescending(x => x.DistanceKm) : q.OrderBy(x => x.DistanceKm),
            "Name" or null or "" => req.Desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            _ => q
        };

        var total = q.Count();
        var page = Math.Max(1, req.Page);
        var size = Math.Clamp(req.PageSize, 1, 200);
        var items = q.Skip((page - 1) * size).Take(size).ToList();
        return new PagedResult<Tour>(items, page, size, total);
    }

    public async Task<Tour> CreateAsync(string name, string? description, string from, string to, string transportType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationFailedException("Name required");
        if (string.IsNullOrWhiteSpace(from)) throw new ValidationFailedException("From required");
        if (string.IsNullOrWhiteSpace(to)) throw new ValidationFailedException("To required");
        if (string.IsNullOrWhiteSpace(transportType)) throw new ValidationFailedException("Transport type required");
        var route = await _mapService.GetRouteAsync(from, to, ct);
        var tour = new Tour(Guid.NewGuid(), name.Trim(), description?.Trim(), from.Trim(), to.Trim(), transportType.Trim(), route.DistanceKm, route.EstimatedTime, route.Path, route.ImagePath);
        _log.LogInformation("Creating tour {Name}", tour.Name);
        return await _repo.CreateAsync(tour, ct);
    }

    public async Task<Tour> UpdateAsync(Tour tour, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tour.Name)) throw new ValidationFailedException("Name required");
        if (string.IsNullOrWhiteSpace(tour.From)) throw new ValidationFailedException("From required");
        if (string.IsNullOrWhiteSpace(tour.To)) throw new ValidationFailedException("To required");
        if (string.IsNullOrWhiteSpace(tour.TransportType)) throw new ValidationFailedException("Transport type required");
        var route = await _mapService.GetRouteAsync(tour.From, tour.To, ct);
        var updated = tour with
        {
            Name = tour.Name.Trim(),
            From = tour.From.Trim(),
            To = tour.To.Trim(),
            TransportType = tour.TransportType.Trim(),
            DistanceKm = route.DistanceKm,
            EstimatedTime = route.EstimatedTime,
            Route = route.Path,
            RouteImagePath = route.ImagePath
        };
        _log.LogInformation("Updating tour {Id}", tour.Id);
        await _repo.UpdateAsync(updated, ct);
        return updated;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _log.LogInformation("Deleting tour {Id}", id);
        var affected = await _repo.DeleteAsync(id, ct);
        if (affected == 0) throw new NotFoundException(nameof(Tour), id);
    }

    public async Task<IReadOnlyList<TourSummaryDto>> GetSummariesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var tours = await _repo.GetAllAsync(ct);
        var list = new List<TourSummaryDto>();
        foreach (var t in tours)
        {
            var logs = await _logRepo.GetByTourAsync(t.Id, ct);
            var popularity = logs.Count;
            var rating = logs.Any() ? logs.Average(l => l.Rating) : (double?)null;
            var child = ComputeChildFriendliness(logs);
            list.Add(new TourSummaryDto(t.Id, t.Name, t.DistanceKm, popularity, rating, child));
        }
        return list;
    }

    private static bool Contains(Tour t, IReadOnlyList<TourLog> logs, int popularity, double? child, string text)
    {
        return t.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || (t.Description?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
            || t.From.Contains(text, StringComparison.OrdinalIgnoreCase)
            || t.To.Contains(text, StringComparison.OrdinalIgnoreCase)
            || t.TransportType.Contains(text, StringComparison.OrdinalIgnoreCase)
            || popularity.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)
            || (child?.ToString("F1").Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
            || logs.Any(l =>
                (l.Comment?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                || l.Rating.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)
                || l.Difficulty.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)
                || l.TotalDistance.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)
                || l.TotalTime.ToString().Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private static double? ComputeChildFriendliness(IReadOnlyList<TourLog> logs)
    {
        if (logs.Count == 0) return null;
        var avgDiff = logs.Average(l => l.Difficulty);
        var avgDist = logs.Average(l => l.TotalDistance);
        var avgTime = logs.Average(l => l.TotalTime.TotalHours);
        var score = (5 - avgDiff) + Math.Max(0, 5 - avgDist / 10) + Math.Max(0, 5 - avgTime);
        return Math.Round(score / 3, 2);
    }
}

