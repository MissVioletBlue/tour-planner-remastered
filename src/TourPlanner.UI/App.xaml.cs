using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TourPlanner.Application.Interfaces;
using TourPlanner.Application.Services;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.UI.ViewModels;

// Alias, damit "Application" eindeutig der WPF-Typ ist
using WpfApplication = System.Windows.Application;

namespace TourPlanner.UI;

public partial class App : WpfApplication
{
    public static IHost AppHost { get; private set; } = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(cfg =>
        {
            cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            // Infrastructure
            services.AddSingleton<ITourRepository, InMemoryTourRepository>();

            // Application (BL)
            services.AddSingleton<ITourService, TourService>();

            // UI
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<Views.MainWindow>(sp =>
            {
                var vm = sp.GetRequiredService<MainViewModel>();
                return new Views.MainWindow { DataContext = vm };
            });
        })
        .Build();

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost.StartAsync();
        AppHost.Services.GetRequiredService<Views.MainWindow>().Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}