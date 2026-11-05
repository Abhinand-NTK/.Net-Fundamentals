# Personal Details API

A small, production-style **ASP.NET Core (.NET 10) Web API** that stores personal details in **SQLite** through **EF Core**, documented with **Swagger / OpenAPI**. It shows .NET fundamentals in a realistic layered backend.

> **New to the code?** Read the [Project Structure Guide](docs/PROJECT_GUIDE.md). It covers every folder and file, how a request flows through the layers, the data model, validation, error handling, configuration, migrations, and a step-by-step example of adding a feature.

## Run it

```powershell
cd PersonalDetailsApi
dotnet run --launch-profile http
```

Then open **http://localhost:5056**. Swagger UI is served at the root.

- The SQLite file `personal-details.db` is created next to the project, and migrations run automatically at startup.
- In Development, three sample persons are seeded (see `Database:SeedSampleData`).
- Health check: `GET /health`
- Ready-made requests: [PersonalDetailsApi.http](PersonalDetailsApi.http) (VS Code REST Client / Visual Studio)

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/v1/persons` | Search, filter (gender, blood group, marital status, city, age range), sort and page |
| GET | `/api/v1/persons/{id}` | One person with all details |
| POST | `/api/v1/persons` | Create a person (201 + `Location` header) |
| PUT | `/api/v1/persons/{id}` | Replace a person and all child collections |
| DELETE | `/api/v1/persons/{id}` | Soft delete (204) |
| GET | `/api/v1/persons/statistics` | Counts by gender, blood group, nationality and age group |

Errors are returned as RFC 7807 `ProblemDetails` (400 validation, 404 not found, 409 duplicate e-mail, 500 generic).

## What is stored

**Person:** name parts, date of birth (age is calculated), place of birth, gender, marital status, blood group, nationality, e-mail, phones, languages, notes
**plus** addresses, emergency contacts, education, employment history and identity documents (document numbers are masked in responses).

## Project layout

```
PersonalDetailsApi/
├── Controllers/        HTTP endpoints only, no business logic
├── Services/           Business rules (unique e-mail, statistics)
├── Repositories/       Data access: generic Repository<T> + PersonRepository
├── Data/               AppDbContext, Fluent API configurations, migrations, seeding
├── Domain/             Entities, enums, base classes
├── Contracts/          Request/response DTOs (records), PagedResult<T>
├── Mapping/            Entity <-> DTO extension methods
├── Common/             Exceptions, global exception handler, validation attributes, extensions
└── Configuration/      DI registration, Swagger setup, options classes
```

Request flow: **Controller → Service → Repository → AppDbContext → SQLite**

## .NET fundamentals shown, and where

| Concept | Where |
|---|---|
| Abstract classes, inheritance, interfaces | `Domain/Common/BaseEntity.cs`, `Domain/Entities/PersonDetails.cs` |
| Generics and generic constraints | `Repositories/Repository.cs`, `Contracts/Responses/PagedResult.cs`, `Data/Configurations/PersonDetailsConfiguration.cs` |
| Polymorphism (abstract members) | `Common/Exceptions/AppExceptions.cs` |
| Records, `init` properties, immutability | `Contracts/` |
| Extension methods | `Common/Extensions/`, `Mapping/PersonMappings.cs`, `Configuration/ServiceCollectionExtensions.cs` |
| LINQ (query and in-memory: `GroupBy`, `ToDictionary`, ...) | `Repositories/PersonRepository.cs`, `Services/PersonService.cs` |
| Expression trees | `QueryableExtensions.OrderBy`, `IRepository.ExistsAsync` |
| Pattern matching: switch expressions, relational and property patterns | `PersonService.ToAgeGroup`, `PersonRepository.SearchAsync`, `GlobalExceptionHandler` |
| async/await and `CancellationToken` | Every layer |
| Dependency injection: singleton and scoped lifetimes | `Configuration/ServiceCollectionExtensions.cs` |
| Primary constructors | Services, repositories, controller |
| Options pattern with validation | `Configuration/ApiDocumentationOptions.cs` |
| Configuration per environment | `appsettings.json`, `appsettings.Development.json` |
| Logging (structured) | `PersonService`, `GlobalExceptionHandler` |
| Validation: data annotations, custom attributes, `IValidatableObject` | `Contracts/Requests/PersonRequest.cs`, `Common/Validation/` |
| Source-generated regex | `PhoneNumberAttribute` |
| Iterators (`yield return`) | `Data/DatabaseInitializer.cs`, `PersonRequest.Validate` |
| Nullable reference types | Enabled project-wide |
| Testable time (`TimeProvider`) | `AppDbContext`, `PersonService` |
| Middleware pipeline and global error handling | `Program.cs`, `Common/Errors/GlobalExceptionHandler.cs` |
| EF Core: Fluent API, migrations, relationships, query filters, soft delete, auditing, value converters, JSON primitive collections, split queries | `Data/` |

## Database migrations

The `dotnet-ef` tool is pinned in the solution's tool manifest.

```powershell
dotnet tool restore
dotnet ef migrations add <Name> --output-dir Data/Migrations   # run inside PersonalDetailsApi/
dotnet ef database update
```
