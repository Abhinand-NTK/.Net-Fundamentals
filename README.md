# .Net-Fundamentals

| Project | What it teaches |
|---|---|
| `CSharpBasics` | C# language basics: syntax, types, operators, control flow, methods, arrays |
| `DotNetProjectBasics` | How a .NET project works: SDK/runtime, `.csproj`, entry point, build output, NuGet, project references, configuration, dependency injection, logging |
| `SharedLibrary` | A small class library used by `DotNetProjectBasics` to show project references |
| `PersonalDetailsApi` | ASP.NET Core Web API (SQLite + EF Core + Swagger) for storing personal details, applying the fundamentals in a layered backend. See [its README](PersonalDetailsApi/README.md) |
| `PersonalDetailsWeb` | React + TypeScript frontend (Vite) for `PersonalDetailsApi`: search, view, create, edit, delete, statistics. See [its README](PersonalDetailsWeb/README.md) |

```powershell
dotnet build Fundamentals.slnx                    # build everything
dotnet run --project CSharpBasics
dotnet run --project DotNetProjectBasics
dotnet run --project DotNetProjectBasics -- hello --Store:Name=FromArgs   # pass args
$env:DOTNET_ENVIRONMENT = "Development"; dotnet run --project DotNetProjectBasics  # use appsettings.Development.json
```

Run the Personal Details app (API + React frontend) in two terminals:

```powershell
dotnet run --project PersonalDetailsApi --launch-profile http   # API + Swagger on http://localhost:5056
cd PersonalDetailsWeb; npm install; npm run dev                  # UI on http://localhost:5173 (Node 20.19+/22.12+)
```

How the two fit together is explained in the [Project Structure Guide](PersonalDetailsApi/docs/PROJECT_GUIDE.md#11-the-react-frontend).
