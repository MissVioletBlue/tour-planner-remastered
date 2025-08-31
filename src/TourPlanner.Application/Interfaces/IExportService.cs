namespace TourPlanner.Application.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportToursAsync(IEnumerable<Guid> ids, string format, CancellationToken ct = default);
    Task<int> ImportToursAsync(byte[] content, string format, CancellationToken ct = default);
}
