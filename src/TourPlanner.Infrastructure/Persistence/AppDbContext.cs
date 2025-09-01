using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
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
            e.Property(x => x.RouteImagePath).HasMaxLength(500);
            e.Property(x => x.Route)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<(double Lat, double Lng)>>(v) ?? new())
                .HasColumnType("jsonb");
            e.HasIndex(x => x.Name);
            e.HasIndex(x => x.DistanceKm);
        });

        b.Entity<TourLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Rating).IsRequired();
            e.HasIndex(x => new { x.TourId, x.Date });
            e.HasIndex(x => new { x.TourId, x.Rating });
        });
    }
}