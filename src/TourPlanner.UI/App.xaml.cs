using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using log4net;
using log4net.Config;
using TourPlanner.Application.Interfaces;
using TourPlanner.Application.Services;
using TourPlanner.UI.ViewModels;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Persistence;

// Alias, damit "Application" eindeutig der WPF-Typ ist
using WpfApplication = System.Windows.Application;

namespace TourPlanner.UI;

public partial class App : WpfApplication
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(App));

    public static IHost AppHost { get; private set; } = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(cfg =>
        {
            cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddInfrastructure(context.Configuration);

            services.AddSingleton<ITourService, TourService>();
            services.AddSingleton<ITourLogService, TourLogService>();

            services.AddSingleton<MapViewModel>();
            services.AddSingleton<TourListViewModel>();
            services.AddSingleton<TourDetailViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<Views.MainWindow>(sp =>
                new Views.MainWindow { DataContext = sp.GetRequiredService<MainViewModel>() });
        })
        .Build();

    protected override async void OnStartup(StartupEventArgs e)
    {
        XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

        this.DispatcherUnhandledException += (s, args) =>
        {
            Log.Error("Unhandled UI exception", args.Exception);
            MessageBox.Show(args.Exception.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        await AppHost.StartAsync();

        using (var scope = AppHost.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateTable)
            {
                Log.Warn("Database already initialized, skipping migrations", ex);
                await db.Database.EnsureCreatedAsync();
            }
        }

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
