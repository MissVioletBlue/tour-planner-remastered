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

## Konfiguration / Dev
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

### Postgres & EF Core
1. Postgres starten (z.B. via Docker):
   ```bash
   docker run --name tourplanner-db -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres
   ```
2. EF-Migrationen anwenden:
   ```bash
   dotnet ef database update --project src/TourPlanner.Infrastructure --startup-project src/TourPlanner.UI
   ```

## Tests
```bash
dotnet test
```

## Nächste Schritte (Tag 2 Vorschau)
  - EF Core/Dapper installieren
  - `TourPlanner.Infrastructure` an Postgres anbinden (Migrations/Repos)
  - `TourPlanner.Application`-Services mit echten Repos verdrahten

## DI-Switch (InMemory ↔ Ef)

Standardmäßig nutzt die UI ein InMemory-Repository. Für die Umstellung auf EF-Core müssen `AppDbContext` und das EF-Repository registriert werden:

```csharp
// InMemory (default)
services.AddSingleton<ITourRepository, InMemoryTourRepository>();

// EF-Core
services.AddDbContext<AppDbContext>(o =>
{
    var conn = context.Configuration.GetConnectionString("Postgres");
    o.UseNpgsql(conn);
});
services.AddScoped<ITourRepository, EfTourRepository>();
```

Je nach Bedarf den gewünschten Block aktivieren und den anderen entfernen oder auskommentieren.
