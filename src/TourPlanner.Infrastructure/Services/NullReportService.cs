using TourPlanner.Application.Interfaces;

namespace TourPlanner.Infrastructure.Services;

public sealed class NullReportService : IReportService
{
    public Task<byte[]> BuildTourReportAsync(Guid tourId, CancellationToken ct = default)
        => Task.FromResult(Array.Empty<byte>());

    public Task<byte[]> BuildSummaryReportAsync(CancellationToken ct = default)
        => Task.FromResult(Array.Empty<byte>());
}
