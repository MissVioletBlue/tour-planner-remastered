namespace TourPlanner.Domain.Entities;

public record TourLog(Guid Id, Guid TourId, DateTime Date, string? Notes, int Rating);
