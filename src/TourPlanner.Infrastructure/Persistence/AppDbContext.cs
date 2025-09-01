using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TourPlanner.Domain.Entities;

namespace TourPlanner.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourLog> TourLogs => Set<TourLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    private static string SerializeRoute(List<(double Lat, double Lng)> route) =>
        JsonSerializer.Serialize(route);

    private static List<(double Lat, double Lng)> DeserializeRoute(string json) =>
        JsonSerializer.Deserialize<List<(double Lat, double Lng)>>(json) ?? new();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Tour>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.DistanceKm).HasPrecision(9, 2);
            e.Property(x => x.RouteImagePath).HasMaxLength(500);
            var comparer = new ValueComparer<List<(double Lat, double Lng)>>(
                (l1, l2) => l1.SequenceEqual(l2),
                l => l.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                l => l.ToList());

            e.Property(x => x.Route)
                .HasConversion(
                    v => SerializeRoute(v),
                    v => DeserializeRoute(v))
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(comparer);
            e.HasIndex(x => x.Name);
            e.HasIndex(x => x.DistanceKm);
        });

        b.Entity<TourLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Rating).IsRequired();
            e.Property(x => x.TotalDistance).HasPrecision(9, 2);
            e.HasIndex(x => new { x.TourId, x.Date });
            e.HasIndex(x => new { x.TourId, x.Rating });
        });
    }
}
