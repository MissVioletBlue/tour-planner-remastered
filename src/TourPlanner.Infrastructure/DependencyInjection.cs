using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.Application.Interfaces;
using TourPlanner.Infrastructure.Persistence;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.Infrastructure.Services;

namespace TourPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var useEf = cfg.GetValue<bool>("Data:UseEf", false);
        if (useEf)
        {
            var cs = cfg.GetConnectionString("Postgres")!;
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs));
            services.AddScoped<ITourRepository, EfTourRepository>();
            services.AddScoped<ITourLogRepository, EfTourLogRepository>();
            services.AddSingleton<IReportService, ReportService>();
        }
        else
        {
            services.AddSingleton<ITourRepository, InMemoryTourRepository>();
            services.AddSingleton<ITourLogRepository, InMemoryTourLogRepository>();
            services.AddSingleton<IReportService, NullReportService>();
        }

        var apiKey = cfg["OpenRouteService:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddSingleton<IMapService, StubMapService>();
        }
        else
        {
            services.AddHttpClient<IMapService, MapService>(c =>
            {
                c.BaseAddress = new Uri("https://api.openrouteservice.org/");
                c.Timeout = TimeSpan.FromSeconds(20);
                c.DefaultRequestHeaders.Accept.Clear();
                c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });
        }

        return services;
    }
}
