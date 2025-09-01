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
        }
        else
        {
            services.AddSingleton<ITourRepository, InMemoryTourRepository>();
        }

        services.AddSingleton<IMapService, MapService>();

        return services;
    }
}
