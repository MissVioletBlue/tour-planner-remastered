namespace TourPlanner.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> BuildTourReportAsync(Guid tourId, CancellationToken ct = default);
    Task<byte[]> BuildSummaryReportAsync(CancellationToken ct = default);
}
