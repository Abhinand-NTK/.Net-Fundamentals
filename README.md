# .Net-Fundamentals

| Project | What it teaches |
|---|---|
| `CSharpBasics` | C# language basics: syntax, types, operators, control flow, methods, arrays |
| `DotNetProjectBasics` | How a .NET project works: SDK/runtime, `.csproj`, entry point, build output, NuGet, project references, configuration, dependency injection, logging |
| `SharedLibrary` | A small class library used by `DotNetProjectBasics` to show project references |
| `PersonalDetailsApi` | ASP.NET Core Web API (SQLite + EF Core + Swagger) for storing personal details, applying the fundamentals in a layered backend. See [its README](PersonalDetailsApi/README.md) |

```powershell
dotnet build Fundamentals.slnx                    # build everything
dotnet run --project CSharpBasics
dotnet run --project DotNetProjectBasics
dotnet run --project DotNetProjectBasics -- hello --Store:Name=FromArgs   # pass args
$env:DOTNET_ENVIRONMENT = "Development"; dotnet run --project DotNetProjectBasics  # use appsettings.Development.json
```
