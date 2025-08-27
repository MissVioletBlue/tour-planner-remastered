# TourPlanner – Tag 1 Skeleton (JetBrains Rider)

Dieser Tag‑1‑Stand liefert dir:
- Solution-Struktur (UI / Application (BL) / Infrastructure (DAL) / Domain / Tests)
- **WPF** (XAML, markup-based UI) + **MVVM**-Skelett
- **Dependency Injection** via `Microsoft.Extensions.Hosting`
- `appsettings.json` inkl. Platzhalter für DB-Connection + LogLevel

> Hinweis: Tag 2 ergänzt Postgres + OR-Mapper (EF Core oder Dapper).

## Öffnen in JetBrains Rider
1. **Rider → Open** und den Ordner `src/TourPlanner.UI` öffnen (oder `src` und die Projekte manuell als Solution anlegen).
2. NuGet-Pakete **Restore** (Rider macht das idR. automatisch).
3. `TourPlanner.UI` als **Startup Project** setzen.
4. Starten (Run). Ein simples Hauptfenster erscheint, gebunden an `MainViewModel`.

## Projekte
- `TourPlanner.Domain` – Domain-Model (Entities)
- `TourPlanner.Application` – Business-Logik (Services/Interfaces)
- `TourPlanner.Infrastructure` – Datenzugriff (Repos – Tag 1: InMemory-Stub)
- `TourPlanner.UI` – WPF UI (MVVM, DI, Konfiguration)
- `TourPlanner.Tests` – xUnit (Beispieltest)

## Konfiguration
Passe in `src/TourPlanner.UI/appsettings.json` an:
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=tourplanner;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": "Information"
  }
}
```

## Nächste Schritte (Tag 2 Vorschau)
  - EF Core/Dapper installieren
  - `TourPlanner.Infrastructure` an Postgres anbinden (Migrations/Repos)
  - `TourPlanner.Application`-Services mit echten Repos verdrahten

## Späterer DI-Switch

Nach dem Merge mit WPF-UI kann in `App.xaml.cs` vom derzeitigen `InMemoryTourRepository` auf `EfTourRepository` umgestellt werden:

```csharp
// services.AddSingleton<ITourRepository, InMemoryTourRepository>();

// services.AddDbContext<AppDbContext>(o =>
// {
//     var conn = context.Configuration.GetConnectionString("Postgres");
//     o.UseNpgsql(conn);
// });
// services.AddScoped<ITourRepository, EfTourRepository>();
```

Diese Anleitung bleibt kommentiert, damit Waldo weiterhin das InMemory-Repo nutzt.