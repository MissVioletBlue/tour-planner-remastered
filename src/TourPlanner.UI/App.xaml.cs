using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TourPlanner.Application.Interfaces;
using TourPlanner.Application.Services;
using TourPlanner.UI.ViewModels;
using TourPlanner.Infrastructure;

// Alias, damit "Application" eindeutig der WPF-Typ ist
using WpfApplication = System.Windows.Application;

namespace TourPlanner.UI;

public partial class App : WpfApplication
{
    public static IHost AppHost { get; private set; } = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(cfg =>
        {
            cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .UseSerilog((ctx, log) =>
        {
            log.MinimumLevel.Information()
               .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddInfrastructure(context.Configuration);

            services.AddSingleton<ITourService, TourService>();
            services.AddSingleton<ITourLogService, TourLogService>();

            services.AddSingleton<MapViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<Views.MainWindow>(sp =>
                new Views.MainWindow { DataContext = sp.GetRequiredService<MainViewModel>() });
        })
        .Build();

    protected override async void OnStartup(StartupEventArgs e)
    {
        this.DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "Unhandled UI exception");
            MessageBox.Show(args.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

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