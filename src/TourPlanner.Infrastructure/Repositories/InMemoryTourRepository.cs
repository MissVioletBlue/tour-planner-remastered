using System;
using System.Collections.Generic;
using System.Linq;
using TourPlanner.Domain.Entities;
using TourPlanner.Application.Interfaces;
using TourPlanner.Application.Contracts;

namespace TourPlanner.Infrastructure.Repositories;

public sealed class InMemoryTourRepository : ITourRepository
{
    private readonly List<Tour> _tours = new();

    public Task<IReadOnlyList<Tour>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<Tour>)_tours.OrderBy(t => t.Name).ToList());
    }

    public Task<PagedResult<Tour>> SearchAsync(SearchRequest r, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IEnumerable<Tour> q = _tours;

        if (!string.IsNullOrWhiteSpace(r.Text))
            q = q.Where(t =>
                t.Name.Contains(r.Text, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(r.Text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                t.From.Contains(r.Text, StringComparison.OrdinalIgnoreCase) ||
                t.To.Contains(r.Text, StringComparison.OrdinalIgnoreCase) ||
                t.TransportType.Contains(r.Text, StringComparison.OrdinalIgnoreCase));

        q = (r.SortBy) switch
        {
            "DistanceKm" => r.Desc ? q.OrderByDescending(x => x.DistanceKm) : q.OrderBy(x => x.DistanceKm),
            "Name" or null or "" => r.Desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            _ => q
        };

        var total = q.Count();
        var page = Math.Max(1, r.Page);
        var size = Math.Clamp(r.PageSize, 1, 200);
        var items = q.Skip((page - 1) * size).Take(size).ToList();

        return Task.FromResult(new PagedResult<Tour>(items, page, size, total));
    }

    public Task<Tour> CreateAsync(Tour tour, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _tours.Add(tour);
        return Task.FromResult(tour);
    }

    public Task UpdateAsync(Tour tour, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var idx = _tours.FindIndex(t => t.Id == tour.Id);
        if (idx >= 0) _tours[idx] = tour;
        return Task.CompletedTask;
    }

    public Task<int> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var removed = _tours.RemoveAll(t => t.Id == id);
        return Task.FromResult(removed);
    }

    public Task<IReadOnlyList<TourSummaryDto>> GetSummariesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var list = _tours
            .OrderBy(t => t.Name)
            .Select(t => new TourSummaryDto(t.Id, t.Name, t.DistanceKm, 0, null, null))
            .ToList();
        return Task.FromResult((IReadOnlyList<TourSummaryDto>)list);
    }
}