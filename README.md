# Tour Planner

## Architecture
- **Domain**: entities like `Tour` and `TourLog`.
- **Application**: service layer with DTOs, interfaces and business logic.
- **Infrastructure**: EF Core repositories, additional services (export, reporting).
- **UI**: WPF host wiring up DI. Uses in-memory repo by default.

## OpenRouteService
The map tab can draw routes using the [OpenRouteService](https://openrouteservice.org/) API. To enable it, supply an API key in the UI configuration:

```
{
  "OpenRouteService": { "ApiKey": "YOUR_KEY" }
}
```

When no key is provided the application registers a stub map service which raises an error, indicating that routing requires an API key.

## Database & Migrations
EF Core 8 with Npgsql. When using PostgreSQL, register `AppDbContext` and repositories in DI and run migrations:

```bash
dotnet ef migrations add Init --project src/TourPlanner.Infrastructure --startup-project src/TourPlanner.UI
```

Optional auto-migrate on startup:

```csharp
using (var scope = App.AppHost.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

## Switching Repositories
The UI currently uses the in-memory repository. To switch to EF + Postgres, replace DI registrations in `App.xaml.cs`:

```csharp
services.AddDbContext<AppDbContext>(o => o.UseNpgsql("<conn-string>"));
services.AddScoped<ITourRepository, EfTourRepository>();
```

## Testing
Run unit tests:

```bash
dotnet test
```

## Export / Import
`IExportService` exports selected tours (with logs) to JSON or simple CSV and can import JSON back:

```csharp
var bytes = await export.ExportToursAsync(ids, "json");
await export.ImportToursAsync(bytes, "json");
```

## PDF Reporting
`IReportService` builds a single-tour PDF using QuestPDF:

```csharp
var pdf = await reports.BuildTourReportAsync(tourId);
File.WriteAllBytes("tour.pdf", pdf);
```

## Publish
Create a publish folder for the UI:

```bash
dotnet publish src/TourPlanner.UI -c Release -r win-x64 --self-contained false -o publish/win
```

