using Microsoft.EntityFrameworkCore;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourLog> TourLogs => Set<TourLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Tour>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.DistanceKm).HasPrecision(9, 2);
            e.HasIndex(x => x.Name);
        });

        b.Entity<TourLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TourId, x.Date });
            e.Property(x => x.Rating).IsRequired();
        });
    }
}