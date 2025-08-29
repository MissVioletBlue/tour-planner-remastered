using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TourPlanner.Infrastructure.Persistence;

#nullable disable

namespace TourPlanner.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TourPlanner.Domain.Entities.Tour", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<string>("Description")
                    .HasColumnType("text");

                b.Property<double>("DistanceKm")
                    .HasPrecision(9, 2)
                    .HasColumnType("numeric(9,2)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.HasKey("Id");

                b.HasIndex("DistanceKm");

                b.HasIndex("Name");

                b.ToTable("Tours");
            });

            modelBuilder.Entity("TourPlanner.Domain.Entities.TourLog", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<DateTime>("Date")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Notes")
                    .HasColumnType("text");

                b.Property<int>("Rating")
                    .HasColumnType("integer");

                b.Property<Guid>("TourId")
                    .HasColumnType("uuid");

                b.HasKey("Id");

                b.HasIndex("TourId", "Date");

                b.HasIndex("TourId", "Rating");

                b.ToTable("TourLogs");
            });
        }
    }
}

