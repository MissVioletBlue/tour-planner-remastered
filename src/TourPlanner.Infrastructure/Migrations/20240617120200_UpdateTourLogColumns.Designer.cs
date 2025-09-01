using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TourPlanner.Infrastructure.Persistence;

#nullable disable

namespace TourPlanner.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240617120200_UpdateTourLogColumns")]
    partial class UpdateTourLogColumns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                b.Property<TimeSpan>("EstimatedTime")
                    .HasColumnType("interval");

                b.Property<string>("From")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)");

                b.Property<string>("Route")
                    .HasColumnType("jsonb");

                b.Property<string>("RouteImagePath")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("character varying(500)");

                b.Property<string>("To")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("TransportType")
                    .IsRequired()
                    .HasColumnType("text");

                b.HasKey("Id");

                b.HasIndex("DistanceKm");

                b.HasIndex("Name");

                b.ToTable("Tours");
            });

            modelBuilder.Entity("TourPlanner.Domain.Entities.TourLog", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<string>("Comment")
                    .HasColumnType("text");

                b.Property<DateTime>("Date")
                    .HasColumnType("timestamp with time zone");

                b.Property<int>("Difficulty")
                    .HasColumnType("integer");

                b.Property<int>("Rating")
                    .HasColumnType("integer");

                b.Property<double>("TotalDistance")
                    .HasPrecision(9, 2)
                    .HasColumnType("numeric(9,2)");

                b.Property<TimeSpan>("TotalTime")
                    .HasColumnType("interval");

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
