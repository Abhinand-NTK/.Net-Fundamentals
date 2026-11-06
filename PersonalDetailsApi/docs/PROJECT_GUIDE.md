# Personal Details API: Project Structure Guide

This guide explains how the project is organised, what each file does, and how a request moves through the code. Read it alongside the source. Every file mentioned here is a link.

**Contents**

1. [The big picture](#1-the-big-picture)
2. [Folder-by-folder tour](#2-folder-by-folder-tour)
3. [What happens when the app starts](#3-what-happens-when-the-app-starts)
4. [Following one request end to end](#4-following-one-request-end-to-end)
5. [The data model](#5-the-data-model)
6. [Validation: three layers](#6-validation-three-layers)
7. [Error handling](#7-error-handling)
8. [Configuration and environments](#8-configuration-and-environments)
9. [Database and migrations](#9-database-and-migrations)
10. [How to add a new feature](#10-how-to-add-a-new-feature)
11. [The React frontend](#11-the-react-frontend)
12. [Running the full stack](#12-running-the-full-stack)
13. [Suggested reading order](#13-suggested-reading-order)
14. [Glossary](#14-glossary)

---

## 1. The big picture

The project is a **layered** Web API with a **React single-page app** in front of it. Each layer has one job and only talks to the layer directly below it.

```
┌───────────────────────────────────────┐
│ Browser  PersonalDetailsWeb (React)   │  Pages, forms, charts. Calls the API with fetch().
└───────────────────┬───────────────────┘
                    │ /api/v1/persons...  (Vite dev server on :5173 proxies to :5056)
                    ▼
            HTTP request (JSON)
                    │
                    ▼
┌───────────────────────────────────────┐
│ Controllers/      PersonsController   │  Receives HTTP, returns HTTP. No business logic.
└───────────────────┬───────────────────┘
                    │ DTOs (Contracts/)
                    ▼
┌───────────────────────────────────────┐
│ Services/         PersonService       │  Business rules: unique e-mail, statistics, ...
└───────────────────┬───────────────────┘
                    │ Entities (Domain/)  ◄── Mapping/ converts DTO ⇄ Entity
                    ▼
┌───────────────────────────────────────┐
│ Repositories/     PersonRepository    │  Builds database queries (LINQ).
└───────────────────┬───────────────────┘
                    ▼
┌───────────────────────────────────────┐
│ Data/             AppDbContext        │  EF Core: tracks changes, writes SQL.
└───────────────────┬───────────────────┘
                    ▼
            SQLite file: personal-details.db
```

Code that every layer uses (exceptions, validation attributes, helper extensions) lives in `Common/`. Start-up wiring lives in `Configuration/` and `Program.cs`.

The frontend lives in a separate folder, [`PersonalDetailsWeb/`](../../PersonalDetailsWeb/), next to the API. It knows nothing about C#, EF Core or SQLite. Its only contract with the backend is the HTTP API: the JSON shapes in `Contracts/` and the ProblemDetails error format. See [section 11](#11-the-react-frontend).

### Why split it like this?

| Benefit | Example in this project |
|---|---|
| **Each piece is easy to find** | Wrong SQL filter? Look in the repository. Wrong HTTP status? Look in the controller or exception. |
| **Layers can change independently** | You could swap SQLite for PostgreSQL by changing only `AddPersistence`. Controllers and services don't change. |
| **The API shape is separate from the table shape** | `PersonDetailResponse` has `Age` and `MaskedDocumentNumber`, which are not database columns. |
| **Testable** | `PersonService` depends on the `IPersonRepository` interface and `TimeProvider`, so a test can pass in fakes. |

### The two kinds of "model"

Beginners often wonder why there are two similar sets of classes:

- **Entities** ([Domain/](../Domain/)) describe *how data is stored*. EF Core maps them to tables.
- **DTOs** ([Contracts/](../Contracts/)), short for *Data Transfer Objects*, describe *what the API accepts and returns*.

Keeping them separate means a client can never set `IsDeleted` or `CreatedAtUtc` directly, and a database change doesn't silently change the public API.

---

## 2. Folder-by-folder tour

```
PersonalDetailsApi/
├── Program.cs                     Entry point: registers services, builds the pipeline
├── PersonalDetailsApi.csproj      Project file: target framework, NuGet packages
├── appsettings.json               Settings for all environments
├── appsettings.Development.json   Overrides used when running locally
├── PersonalDetailsApi.http        Sample requests you can send from the editor
├── Properties/launchSettings.json Ports and environment for `dotnet run`
│
├── Controllers/                   HTTP layer
├── Services/                      Business layer
├── Repositories/                  Data-access layer
├── Data/                          EF Core: DbContext, mappings, migrations, seed data
├── Domain/                        Entities and enums (the core model)
├── Contracts/                     Request / response DTOs
├── Mapping/                       DTO ⇄ Entity conversion
├── Common/                        Cross-cutting helpers used by many layers
├── Configuration/                 DI registration and Swagger set-up
└── docs/                          This guide
```

### 2.1 Root files

| File | Purpose |
|---|---|
| [Program.cs](../Program.cs) | The app's entry point. It has three numbered sections: **register services**, **prepare the database**, **configure the HTTP pipeline**. It ends with `public partial class Program;` so future integration tests can reference it. |
| [PersonalDetailsApi.csproj](../PersonalDetailsApi.csproj) | Uses `Microsoft.NET.Sdk.Web`. `GenerateDocumentationFile` turns `///` comments into an XML file that Swagger reads. `TreatWarningsAsErrors` keeps the build clean. |
| [appsettings.json](../appsettings.json) / [appsettings.Development.json](../appsettings.Development.json) | Connection string, seeding switch, Swagger text, log levels. See [section 8](#8-configuration-and-environments). |
| [PersonalDetailsApi.http](../PersonalDetailsApi.http) | Click "Send Request" in VS Code (REST Client extension) or Visual Studio. The create request is named, so later requests reuse its `id`. |
| [Properties/launchSettings.json](../Properties/launchSettings.json) | Defines the `http` (port 5056) and `https` (port 7284) profiles and sets `ASPNETCORE_ENVIRONMENT=Development`. Only used by `dotnet run` and IDEs, not in production. |

### 2.2 `Controllers/`: the HTTP layer

| File | Purpose |
|---|---|
| [PersonsController.cs](../Controllers/PersonsController.cs) | Six endpoints under `api/v1/persons`. Each method is one line or two: call the service, then wrap the result in `Ok`, `CreatedAtRoute` or `NoContent`. |

Things to notice:

- `[ApiController]` turns on automatic model validation. If the body is invalid, the method never runs and a 400 is returned.
- `[Route("api/v1/persons")]` puts the version in the URL.
- `{id:guid}` is a **route constraint**. `/persons/statistics` can't be mistaken for an id because "statistics" isn't a GUID.
- `[ProducesResponseType<T>(...)]` and `///` comments are what Swagger displays. They don't change behaviour.
- `CancellationToken ct` is filled in by ASP.NET Core. If the client disconnects, the database query is cancelled.
- The controller's constructor parameter `IPersonService personService` is a **primary constructor**. DI supplies the service.

### 2.3 `Services/`: business rules

| File | Purpose |
|---|---|
| [PersonService.cs](../Services/PersonService.cs) | Contains `IPersonService` (the contract) and `PersonService` (the implementation). |

Responsibilities:

- **Unique e-mail check** (`EnsureEmailIsUniqueAsync`), which throws `ConflictException` → 409.
- **Not-found handling**: `?? throw new NotFoundException(...)` → 404.
- **Calls mapping** to turn entities into response DTOs.
- **Statistics**: loads a slim projection (`PersonStatRow`) and groups it in memory with LINQ (`GroupBy`, `ToDictionary`). Age groups use a **relational pattern** switch (`< 18 => "Under 18"`).
- **Logging** of create, update and delete with structured placeholders (`{PersonId}`).

The service never touches `HttpContext` and never references EF Core directly. It only knows the repository interface.

### 2.4 `Repositories/`: data access

| File | Purpose |
|---|---|
| [Repository.cs](../Repositories/Repository.cs) | `IRepository<T>` and `Repository<T>`: **generic** CRUD for any `T : BaseEntity`. Written once, reusable for every entity. |
| [PersonRepository.cs](../Repositories/PersonRepository.cs) | `PersonRepository` **inherits** `Repository<Person>` and adds person-specific queries. |

Key methods in `PersonRepository`:

- `GetWithDetailsAsync`: `Include`s all five child collections. `AsSplitQuery()` runs one SQL query per collection instead of one huge join. `track: false` adds `AsNoTracking()` for read-only calls, which is faster. Updates pass `track: true` so EF can detect changes.
- `SearchAsync`: builds the query step by step. Each `if` adds a `WHERE`. Nothing runs until `ToPagedResultAsync` (this is called **deferred execution**). Age filters are converted to date-of-birth ranges so SQLite can evaluate them.
- `GetStatRowsAsync`: `Select`s only four columns into a record. This is a **projection**.

### 2.5 `Data/`: EF Core

| File | Purpose |
|---|---|
| [AppDbContext.cs](../Data/AppDbContext.cs) | The EF Core context: one `DbSet` per table, plus global rules (see below). |
| [Configurations/PersonConfiguration.cs](../Data/Configurations/PersonConfiguration.cs) | Fluent API mapping for `Person`: column lengths, indexes, the soft-delete query filter, one-to-many relationships. |
| [Configurations/PersonDetailsConfiguration.cs](../Data/Configurations/PersonDetailsConfiguration.cs) | Mappings for the five child entities. They share a generic base class `PersonOwnedConfiguration<T>` (**template method pattern**). The base adds the query filter; each subclass adds its columns. |
| [DatabaseInitializer.cs](../Data/DatabaseInitializer.cs) | Extension method `InitializeDatabaseAsync()`: applies migrations and seeds sample data when enabled. Uses `yield return` to build the sample list. |
| [Migrations/](../Data/Migrations/) | Generated by `dotnet ef`. `*_InitialCreate.cs` holds the `CREATE TABLE` steps. `AppDbContextModelSnapshot.cs` is EF's record of the current model. **Don't edit these by hand.** |

Global rules inside `AppDbContext`:

| Rule | Where | Effect |
|---|---|---|
| Apply all configurations | `OnModelCreating` → `ApplyConfigurationsFromAssembly` | Finds every `IEntityTypeConfiguration<T>` class by reflection. |
| Ids generated in code | `OnModelCreating` loop → `ValueGeneratedNever()` | Ids are Guid v7 set in `BaseEntity`. Without this rule, a new child added during an update would be treated as an existing row. |
| Enums as text | `ConfigureConventions` | Columns store `"Female"`, not `1`, so the database is readable. |
| UTC dates | `ConfigureConventions` → `UtcDateTimeConverter` | SQLite has no time-zone type. This marks values read back as UTC, so the JSON gets its `Z` suffix. |
| Audit timestamps | `SaveChangesAsync` override | Sets `CreatedAtUtc` / `UpdatedAtUtc` automatically. |
| Soft delete | `SaveChangesAsync` override | Converts a `Remove()` on an `ISoftDeletable` into an update that sets `IsDeleted = true`. |

### 2.6 `Domain/`: the core model

| File | Purpose |
|---|---|
| [Common/BaseEntity.cs](../Domain/Common/BaseEntity.cs) | `BaseEntity` (abstract; has `Id`), interfaces `IAuditable` and `ISoftDeletable`, and `AuditableEntity`, which combines them. |
| [Entities/Person.cs](../Domain/Entities/Person.cs) | The **aggregate root**. Holds personal fields and five child lists. `FullName` is computed, not stored. |
| [Entities/PersonDetails.cs](../Domain/Entities/PersonDetails.cs) | `PersonOwnedEntity` (abstract; has `PersonId`) and the five children: `Address`, `EmergencyContact`, `Education`, `Employment`, `IdentityDocument`. |
| [Enums/Enums.cs](../Domain/Enums/Enums.cs) | All enums: `Gender`, `BloodGroup`, `DocumentType`, sort options, and others. |

Inheritance tree:

```
BaseEntity (Id)
├── AuditableEntity (+ CreatedAtUtc, UpdatedAtUtc)   implements IAuditable
│     └── Person                                     also implements ISoftDeletable
└── PersonOwnedEntity (+ PersonId, Person)
      ├── Address
      ├── EmergencyContact
      ├── Education
      ├── Employment
      └── IdentityDocument
```

Entities contain **no** references to ASP.NET Core or EF Core. They are plain C# classes.

### 2.7 `Contracts/`: what the API accepts and returns

| File | Purpose |
|---|---|
| [Requests/PersonRequest.cs](../Contracts/Requests/PersonRequest.cs) | Body for POST and PUT. Includes nested `AddressRequest`, `EmploymentRequest`, and so on. Contains all validation attributes and cross-field rules. `/// <example>` tags fill in the Swagger examples. |
| [Requests/PersonQueryParameters.cs](../Contracts/Requests/PersonQueryParameters.cs) | Query-string options for `GET /persons`: search, filters, sort, page. Bound with `[FromQuery]`. |
| [Responses/PersonResponses.cs](../Contracts/Responses/PersonResponses.cs) | Output records: `PersonSummaryResponse` (list rows), `PersonDetailResponse` (full record), child responses, `PersonStatisticsResponse`. |
| [Responses/PagedResult.cs](../Contracts/Responses/PagedResult.cs) | Generic `PagedResult<T>` with computed `TotalPages`, `HasNextPage`, and a `Map` method to convert the item type. |

Requests use `record` with `init` properties so attributes can be placed on them. Responses use **positional records**: one line, immutable, value equality.

### 2.8 `Mapping/`

| File | Purpose |
|---|---|
| [PersonMappings.cs](../Mapping/PersonMappings.cs) | Extension methods: `request.ToEntity()`, `request.ApplyTo(person)`, `person.ToSummary(today)`, `person.ToDetail(today)`. |

Mapping is also where clean-up happens: names are trimmed, e-mail is lower-cased, duplicate languages are removed, the first address becomes primary if none is flagged, and document numbers are masked.

The mapping is written by hand rather than with a library like AutoMapper, so every conversion is visible and easy to debug.

### 2.9 `Common/`: shared helpers

| File | Purpose |
|---|---|
| [Exceptions/AppExceptions.cs](../Common/Exceptions/AppExceptions.cs) | Abstract `AppException` with `StatusCode` and `Title`. `NotFoundException` (404) and `ConflictException` (409) override them (**polymorphism**). |
| [Errors/GlobalExceptionHandler.cs](../Common/Errors/GlobalExceptionHandler.cs) | Implements `IExceptionHandler`. Catches every unhandled exception and writes a `ProblemDetails` JSON response. |
| [Validation/ValidationAttributes.cs](../Common/Validation/ValidationAttributes.cs) | Custom attributes `[PastDate]` and `[PhoneNumber]`. The phone check uses a source-generated `[GeneratedRegex]`. |
| [Extensions/DateExtensions.cs](../Common/Extensions/DateExtensions.cs) | `AgeOn(today)`, `TimeProvider.Today()`, `Mask()`, `NullIfWhiteSpace()`. |
| [Extensions/QueryableExtensions.cs](../Common/Extensions/QueryableExtensions.cs) | Generic `OrderBy(key, direction)`, `ThenBy(key, direction)` and `ToPagedResultAsync(page, size)`. They work for any `IQueryable<T>`. |

### 2.10 `Configuration/`: start-up wiring

| File | Purpose |
|---|---|
| [ServiceCollectionExtensions.cs](../Configuration/ServiceCollectionExtensions.cs) | `AddPersistence`, `AddApplicationServices`, `AddFrontendCors` and `AddApiDocumentation` register services. `UseApiDocumentation` adds Swagger to the pipeline. Keeps `Program.cs` short. |
| [ApiDocumentationOptions.cs](../Configuration/ApiDocumentationOptions.cs) | Strongly typed settings class bound to the `ApiDocumentation` section (**options pattern**), validated at start-up. |

Service lifetimes registered here:

| Service | Lifetime | Why |
|---|---|---|
| `TimeProvider` | Singleton | Stateless; one instance is enough. |
| `AppDbContext` | Scoped (set by `AddDbContext`) | One per request, so changes from one request never leak into another. |
| `IPersonRepository`, `IPersonService` | Scoped | They use the scoped `AppDbContext`, so they can't live longer than it. |

---

## 3. What happens when the app starts

```
dotnet run
   │
   ▼
Program.cs
 1. WebApplication.CreateBuilder(args)
      reads appsettings.json → appsettings.{Environment}.json → environment variables → command line
 2. builder.Services.AddPersistence(...)        → registers AppDbContext with the SQLite connection string
                    .AddApplicationServices()   → repository, service, TimeProvider, exception handler, health checks
                    .AddFrontendCors(...)       → CORS policy from Cors:AllowedOrigins
                    .AddApiDocumentation(...)   → options + SwaggerGen
    builder.Services.AddControllers().AddJsonOptions(...)  → enums as strings, omit nulls
 3. builder.Build()                             → DI container is now locked
 4. app.InitializeDatabaseAsync()               → creates a scope, runs migrations, seeds if enabled
 5. Middleware pipeline (runs in this order for every request):
      UseExceptionHandler  → catches errors from everything after it
      UseStatusCodePages   → gives empty 4xx/5xx responses a ProblemDetails body
      UseCors              → answers preflight requests and adds Access-Control-* headers for allowed origins
      UseApiDocumentation  → serves /swagger/v1/swagger.json and the UI at /
      MapControllers       → routes /api/v1/persons/... to PersonsController
      MapHealthChecks      → /health checks that the database is reachable
 6. app.RunAsync()                              → starts Kestrel on http://localhost:5056
```

**Order matters:** `UseExceptionHandler` comes first so it wraps everything that follows.

---

## 4. Following one request end to end

Example: **`POST /api/v1/persons`** with a JSON body.

| Step | Where | What happens |
|---|---|---|
| 1 | Kestrel / middleware | Request arrives and passes through the exception handler and Swagger middleware. |
| 2 | Routing | Matches `[HttpPost]` on `PersonsController.Create`. |
| 3 | Model binding | The JSON body is deserialised into `PersonRequest`. Enum text like `"Female"` becomes `Gender.Female`. |
| 4 | Validation | `[ApiController]` runs all attributes and `IValidatableObject.Validate`. **If anything fails, a 400 `ValidationProblemDetails` is returned here, and steps 5 onwards never run.** |
| 5 | Controller | Calls `personService.CreateAsync(request, ct)`. |
| 6 | Service | `EnsureEmailIsUniqueAsync` calls `repository.ExistsAsync(...)`. If the e-mail is taken it throws `ConflictException`, which skips to step 11. |
| 7 | Mapping | `request.ToEntity()` builds a `Person` with child lists. Each object gets a new Guid v7 id. |
| 8 | Repository | `repository.Add(person)` makes EF start tracking the whole graph as *Added*. |
| 9 | DbContext | `SaveChangesAsync` sets `CreatedAtUtc` and writes `INSERT` statements for the person and every child in one transaction. |
| 10 | Back up | Service maps the entity to `PersonDetailResponse`. The controller returns `CreatedAtRoute`, giving **201** plus a `Location: /api/v1/persons/{id}` header. |
| 11 | On error | Any exception bubbles to `GlobalExceptionHandler`, which picks the status code and writes `ProblemDetails`. |

Other operations follow the same path:

- **GET by id:** repository loads with `AsNoTracking`, then `ToDetail`.
- **PUT:** loads *tracked* → `ApplyTo` overwrites fields and replaces the child lists → EF works out the `UPDATE`, `DELETE` (removed children) and `INSERT` (new children).
- **DELETE:** `Remove(person)` → `SaveChangesAsync` turns it into `UPDATE Persons SET IsDeleted = 1`.

---

## 5. The data model

Six tables. Every child table has a `PersonId` foreign key with cascade delete.

```
                         ┌────────────────────────┐
                         │ Persons                │
                         │────────────────────────│
                         │ Id (PK, Guid v7)       │
                         │ FirstName, MiddleName, │
                         │ LastName, PreferredName│
                         │ DateOfBirth, PlaceOf.. │
                         │ Gender, MaritalStatus, │
                         │ BloodGroup, Nationality│
                         │ Email (unique*)        │
                         │ PrimaryPhone, Alt..    │
                         │ Languages (JSON)       │
                         │ Notes                  │
                         │ CreatedAtUtc, Updated..│
                         │ IsDeleted, DeletedAt.. │
                         └───────────┬────────────┘
        ┌──────────────┬─────────────┼──────────────┬──────────────────┐
        ▼              ▼             ▼              ▼                  ▼
  Addresses   EmergencyContacts  Educations    Employments    IdentityDocuments
  Type, Line1,  Name,            Level,        Employer,      DocumentType,
  Line2, City,  Relationship,    Institution,  JobTitle,      DocumentNumber,
  State, Post-  Phone, Email     FieldOfStudy, EmploymentType IssuingCountry,
  alCode,                        StartYear,    StartDate,     IssueDate,
  Country,                       EndYear,      EndDate        ExpiryDate
  IsPrimary                      Grade
```

\* The e-mail index is **filtered** (`WHERE IsDeleted = 0`), so a soft-deleted person's e-mail can be used again.

Design decisions:

| Decision | Reason |
|---|---|
| Guid v7 ids | Safe to expose in URLs (not guessable like 1, 2, 3) and still time-ordered, so they index well. |
| Soft delete + global query filter | Data isn't lost by accident. Every query automatically excludes deleted rows. Children of a deleted person are hidden too. |
| Enums stored as text | The database stays readable with any SQLite viewer. |
| `Languages` as a JSON column | A simple list of strings doesn't need its own table. EF Core supports primitive collections. |
| Age not stored | It changes every year. It is calculated on read from `DateOfBirth` and `TimeProvider`. |
| Computed `IsCurrent`, `FullName` | Derived values are `Ignore`d in the mapping, so they never go out of sync. |

---

## 6. Validation: three layers

| Layer | Where | Examples | Result |
|---|---|---|---|
| **1. Attributes on a single field** | `PersonRequest` properties | `[Required]`, `[StringLength(100)]`, `[EmailAddress]`, `[Range]`, `[RegularExpression]`, custom `[PastDate]` and `[PhoneNumber]` | 400 |
| **2. Cross-field rules** | `Validate()` in each request record (`IValidatableObject`) | Only one primary address; end date after start date; expiry after issue date; no duplicate documents | 400 |
| **3. Rules that need the database** | `PersonService` | E-mail must be unique among active persons | 409 |

The database is the final safety net: the unique index on `Email` and `NOT NULL` columns protect the data even if code is wrong.

Errors in nested lists come back with their path, for example `"Employments[0].EndDate": ["EndDate cannot be before StartDate."]`.

---

## 7. Error handling

All errors use the standard [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807) **ProblemDetails** format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "Person with id '01a1ab38-...' was not found.",
  "instance": "GET /api/v1/persons/01a1ab38-...",
  "traceId": "0HNPR4OMM50MT:00000001"
}
```

How an exception becomes this response:

```
Service: throw new NotFoundException("Person", id)
   │  (bubbles up through the controller; no try/catch anywhere)
   ▼
GlobalExceptionHandler.TryHandleAsync
   switch (exception)
     AppException app          → app.StatusCode, app.Title, app.Message    (404, 409, ...)
     OperationCanceledException → 499 client closed request
     anything else             → 500 "An unexpected error occurred", no detail (never leaks internals)
   logs: Warning for 4xx, Error with stack trace for 5xx
   ▼
IProblemDetailsService writes JSON; AddProblemDetails adds "traceId"
```

**To add a new error type**, create a class that inherits `AppException` and overrides `StatusCode` and `Title`. You don't need to change the handler.

---

## 8. Configuration and environments

ASP.NET Core merges settings in this order, with later sources winning:

```
appsettings.json  →  appsettings.{Environment}.json  →  environment variables  →  command-line args
```

| Key | `appsettings.json` | Development override | Meaning |
|---|---|---|---|
| `ConnectionStrings:PersonalDetails` | `Data Source=personal-details.db` | — | SQLite file path |
| `Database:SeedSampleData` | `false` | `true` | Insert three sample persons if the table is empty |
| `Cors:AllowedOrigins` | `[]` (none) | `["http://localhost:5173"]` | Browser origins allowed to call the API directly. Not needed when the React app uses the Vite proxy (see [section 11](#11-the-react-frontend)) |
| `ApiDocumentation:Enabled` | `false` | `true` | Serve Swagger (off by default for production) |
| `ApiDocumentation:Title`, `Description`, ... | set | — | Text shown in Swagger |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command` | `Warning` | `Information` | `Information` prints every SQL statement, which is useful for learning |

The environment comes from `ASPNETCORE_ENVIRONMENT`, which `launchSettings.json` sets to `Development`.

Examples of overriding settings without editing files:

```powershell
# Use a different database file
dotnet run -- --ConnectionStrings:PersonalDetails="Data Source=other.db"

# Simulate production (Swagger off, no seed data)
$env:ASPNETCORE_ENVIRONMENT = "Production"; dotnet run --launch-profile http
```

`ApiDocumentationOptions` is checked with `ValidateOnStart()`. A missing `Title` stops the app at start-up instead of failing later.

---

## 9. Database and migrations

- The database is a single file, `personal-details.db`, in the project folder. It is git-ignored.
- On every start, `MigrateAsync()` applies any migrations that haven't been run yet. EF records applied migrations in the `__EFMigrationsHistory` table.
- **To reset:** stop the app and delete `personal-details.db*`. It is re-created and re-seeded on the next run.
- **To browse it:** use any SQLite viewer, for example the *SQLite Viewer* extension for VS Code or *DB Browser for SQLite*.

Changing the schema:

```powershell
cd PersonalDetailsApi
dotnet tool restore                                             # installs the pinned dotnet-ef
# 1. edit an entity or configuration
dotnet ef migrations add AddSkills --output-dir Data/Migrations # 2. generate the migration
# 3. review the generated Up()/Down() code
dotnet run                                                      # 4. applied automatically at start-up
```

Useful commands:

```powershell
dotnet ef migrations list
dotnet ef migrations has-pending-model-changes   # did I forget to add a migration?
dotnet ef migrations remove                      # undo the last, unapplied migration
dotnet ef migrations script                      # view the SQL
```

---

## 10. How to add a new feature

Worked example: add **Skills** (name + proficiency level) to a person. The checklist follows the layers from the bottom up.

| # | File | Change |
|---|---|---|
| 1 | `Domain/Enums/Enums.cs` | `public enum Proficiency { Beginner, Intermediate, Advanced, Expert }` |
| 2 | `Domain/Entities/PersonDetails.cs` | `public class Skill : PersonOwnedEntity { public string Name { get; set; } = ""; public Proficiency Level { get; set; } }` |
| 3 | `Domain/Entities/Person.cs` | `public List<Skill> Skills { get; set; } = [];` |
| 4 | `Data/Configurations/PersonDetailsConfiguration.cs` | `public class SkillConfiguration : PersonOwnedConfiguration<Skill>` with `Name` max length. It is picked up automatically. |
| 5 | `Data/Configurations/PersonConfiguration.cs` | `builder.HasMany(p => p.Skills).WithOne(s => s.Person).HasForeignKey(s => s.PersonId).OnDelete(DeleteBehavior.Cascade);` |
| 6 | `Data/AppDbContext.cs` | `public DbSet<Skill> Skills => Set<Skill>();` |
| 7 | Terminal | `dotnet ef migrations add AddSkills --output-dir Data/Migrations` |
| 8 | `Contracts/Requests/PersonRequest.cs` | `SkillRequest` record with validation, and `List<SkillRequest> Skills` on `PersonRequest` |
| 9 | `Contracts/Responses/PersonResponses.cs` | `SkillResponse` record, and add it to `PersonDetailResponse` |
| 10 | `Mapping/PersonMappings.cs` | One `Replace(p.Skills, r.Skills, ...)` line in `ApplyTo`, and the mapping in `ToDetail` |
| 11 | `Repositories/PersonRepository.cs` | `.Include(p => p.Skills)` in `GetWithDetailsAsync` |

The controller and service need **no changes**, because they work with the whole person. This is the payoff of the layering.

For a **new top-level resource** (for example "Departments"), you'd add an entity, a configuration, a `DbSet`, a migration, DTOs, a mapping, an `I…Repository` (inheriting `IRepository<T>`), an `I…Service`, a controller, and two `AddScoped` lines in `AddApplicationServices`.

**To show Skills in the frontend** as well, three more steps follow the backend checklist:

| # | File | Change |
|---|---|---|
| 12 | `PersonalDetailsWeb/src/api/types.ts` | Add `PROFICIENCIES`, `SkillRequest`, `SkillResponse`, and `skills` on `PersonRequest` / `PersonDetailResponse` |
| 13 | `PersonalDetailsWeb/src/pages/PersonFormPage.tsx` | A `SkillForm` type, a `blank.skill()` factory, the two conversions in `fromResponse` / `toRequest`, and one more `<ListEditor>` |
| 14 | `PersonalDetailsWeb/src/pages/PersonDetailPage.tsx` | One more `<Section title="Skills">` |

---

## 11. The React frontend

[`PersonalDetailsWeb/`](../../PersonalDetailsWeb/) is a **React 19 + TypeScript** single-page app built with **Vite**. It covers everything the API can do: search and filter, view, create, edit, delete and statistics.

### 11.1 How the browser reaches the API

In development, two servers run side by side:

```
Browser ──► http://localhost:5173  (Vite dev server: serves the React app, hot reload)
                 │
                 │  any path starting with /api or /health is forwarded (proxy)
                 ▼
            http://localhost:5056  (ASP.NET Core: PersonalDetailsApi)
```

The proxy is configured in [vite.config.ts](../../PersonalDetailsWeb/vite.config.ts). The React code calls **relative** URLs such as `/api/v1/persons`, so the browser only ever sees one origin (`localhost:5173`). That means:

- **No CORS is needed** in development. The browser never makes a cross-origin request.
- The API doesn't need to know the frontend exists.

**CORS** only matters if the browser calls the API on a different origin. That happens when you set `VITE_API_BASE_URL=http://localhost:5056` in `PersonalDetailsWeb/.env.local`, or when the two are hosted on different domains. Then the API must list the frontend's origin in `Cors:AllowedOrigins`. `AddFrontendCors` reads that setting, and `app.UseCors()` applies the policy. The policy also exposes the `Location` header, so a script can read where a newly created person lives.

### 11.2 Folder tour

```
PersonalDetailsWeb/
├── index.html              The single HTML page; loads src/main.tsx
├── vite.config.ts          Dev server port (5173) and the /api proxy
├── package.json            npm scripts (dev, build, preview) and dependencies
├── tsconfig.json           TypeScript compiler options (strict mode)
└── src/
    ├── main.tsx            Entry point: mounts <App /> inside a <BrowserRouter>
    ├── App.tsx             App shell (sidebar navigation) and the client-side routes
    ├── index.css           All styles: colour tokens with a dark-mode variant, responsive layout
    ├── api/
    │   ├── types.ts        TypeScript copies of the C# DTOs and enums in Contracts/ and Domain/Enums
    │   ├── client.ts       fetch wrapper: JSON in/out, ProblemDetails → ApiError, AbortSignal
    │   └── persons.ts      One function per PersonsController endpoint
    ├── hooks/useApi.ts     Load data on mount / when inputs change; aborts stale requests
    ├── components/         Reusable UI: Icon (inline SVG icons), Avatar (coloured initials), Toast
    │                       (confirmation messages), Skeleton (loading placeholders), ErrorMessage,
    │                       Pagination, BarList (bar chart in plain CSS)
    ├── pages/
    │   ├── PersonListPage.tsx    GET /persons with filters, sort and paging (state kept in the URL)
    │   ├── PersonDetailPage.tsx  GET /persons/{id}, plus DELETE
    │   ├── PersonFormPage.tsx    POST (new) and PUT (edit), including all five child lists
    │   └── StatisticsPage.tsx    GET /persons/statistics
    └── utils/format.ts     Enum labels ("APositive" → "A+") and date formatting
```

| Route | Page | API call |
|---|---|---|
| `/` | `PersonListPage` | `GET /api/v1/persons?search=…&page=…` |
| `/persons/new` | `PersonFormPage` | `POST /api/v1/persons` |
| `/persons/:id` | `PersonDetailPage` | `GET /api/v1/persons/{id}`, `DELETE /api/v1/persons/{id}` |
| `/persons/:id/edit` | `PersonFormPage` | `GET` then `PUT /api/v1/persons/{id}` |
| `/statistics` | `StatisticsPage` | `GET /api/v1/persons/statistics` |

### 11.3 How the frontend mirrors the backend

| Backend idea | Frontend counterpart |
|---|---|
| DTO records in `Contracts/` | Interfaces in `src/api/types.ts`. Property names are camelCase because that is how ASP.NET Core serialises JSON. Nullable C# properties are optional (`?`) because the API omits nulls. |
| Enums serialised as strings (`JsonStringEnumConverter`) | String union types built from `as const` arrays, e.g. `GENDERS`. The same arrays fill the `<select>` options. |
| `CancellationToken ct` on every action | `AbortController` in `useApi`. When you leave a page or change a filter, the old request is aborted, and ASP.NET Core sees the token cancelled. |
| `PagedResult<T>` with `TotalPages`, `HasNextPage` | `Pagination` uses those computed fields directly. Nothing is recalculated in the browser. |
| ProblemDetails from `GlobalExceptionHandler` | `ApiError` in `client.ts` carries the parsed body. `ErrorMessage` shows `title`, `detail` and `traceId`. |
| Validation errors keyed by path (`Addresses[0].City`) | `PersonFormPage` lower-cases the keys and shows each message under its field. The form uses `noValidate` on purpose, so you see the **server's** validation from [section 6](#6-validation-three-layers) in action. |
| 409 duplicate e-mail (`ConflictException`) | Shown under the e-mail field. |
| PUT is a full replacement | The edit form sends the whole person, including every child list, every time. |
| Document numbers are masked in responses | When editing, document numbers can't be pre-filled (the browser only gets `****1234`), so the form asks you to re-enter them. This is a real trade-off of masking plus full-replacement PUT. |

### 11.4 Following one request from the browser

Example: clicking **Create person** on `/persons/new`.

| Step | Where | What happens |
|---|---|---|
| 1 | `PersonFormPage.handleSubmit` | `toRequest(form)` turns the form's strings into a `PersonRequest`: blank optional fields become `null`, years become numbers, and languages are split on commas. |
| 2 | `personsApi.create` → `request()` | `fetch('/api/v1/persons', { method: 'POST', body: JSON })` |
| 3 | Vite proxy | Forwards the request to `http://localhost:5056/api/v1/persons`. |
| 4 | API | Runs the same steps as [section 4](#4-following-one-request-end-to-end). |
| 5a | Success (201) | The response body is the new `PersonDetailResponse`. The page navigates to `/persons/{id}`. |
| 5b | Failure (400 / 409) | `request()` throws `ApiError`. The form puts each message under its field and lists them all at the top. |

### 11.5 Tooling notes

- **Node.js 20.19+ or 22.12+** is required by Vite 7. Check with `node -v`. With nvm for Windows: `nvm use 22.22.2`.
- `npm run build` type-checks with `tsc` first, so a mismatch between `types.ts` and how a page uses it fails the build.
- `npm run build` writes static files to `dist/`. `npm run preview` serves them on port 4173, with the same proxy.

---

## 12. Running the full stack

Open two terminals from the repository root:

```powershell
# Terminal 1: API on http://localhost:5056 (Swagger UI at the root)
dotnet run --project PersonalDetailsApi --launch-profile http

# Terminal 2: React app on http://localhost:5173
cd PersonalDetailsWeb
npm install        # first time only
npm run dev
```

Open **http://localhost:5173**. The three seeded sample persons appear in the list.

| Symptom | Cause / fix |
|---|---|
| "Could not reach the API" or 502/504 errors in the browser | The API isn't running, or it's on a different port. Start terminal 1 and check that it listens on 5056. |
| CORS error in the browser console | `VITE_API_BASE_URL` is set, so the browser calls the API directly. Add the frontend's origin to `Cors:AllowedOrigins`, or remove the variable to go back to the proxy. |
| `npm run dev` fails with a syntax or engine error | Node is too old. Switch to Node 22 (`nvm use 22.22.2`). |
| Port 5173 already in use | Another Vite app is running. `strictPort` makes Vite stop instead of silently picking another port, because the CORS origin depends on the port. |

---

## 13. Suggested reading order

If you are new to the code, read the files in this order. Each one builds on the previous.

1. [Program.cs](../Program.cs): the overall shape
2. [Configuration/ServiceCollectionExtensions.cs](../Configuration/ServiceCollectionExtensions.cs): what gets registered
3. [Domain/Common/BaseEntity.cs](../Domain/Common/BaseEntity.cs) → [Domain/Entities/Person.cs](../Domain/Entities/Person.cs) → [PersonDetails.cs](../Domain/Entities/PersonDetails.cs): the model
4. [Controllers/PersonsController.cs](../Controllers/PersonsController.cs): the API surface
5. [Contracts/Requests/PersonRequest.cs](../Contracts/Requests/PersonRequest.cs): input and validation
6. [Services/PersonService.cs](../Services/PersonService.cs): business rules
7. [Mapping/PersonMappings.cs](../Mapping/PersonMappings.cs): conversions
8. [Repositories/Repository.cs](../Repositories/Repository.cs) → [PersonRepository.cs](../Repositories/PersonRepository.cs): queries
9. [Data/AppDbContext.cs](../Data/AppDbContext.cs) → [Configurations/](../Data/Configurations/): persistence
10. [Common/Errors/GlobalExceptionHandler.cs](../Common/Errors/GlobalExceptionHandler.cs): error handling
11. Frontend: [src/api/types.ts](../../PersonalDetailsWeb/src/api/types.ts) → [src/api/client.ts](../../PersonalDetailsWeb/src/api/client.ts) → [src/pages/PersonListPage.tsx](../../PersonalDetailsWeb/src/pages/PersonListPage.tsx) → [src/pages/PersonFormPage.tsx](../../PersonalDetailsWeb/src/pages/PersonFormPage.tsx)

**Hands-on tip:** run the API in Development, then send requests from [PersonalDetailsApi.http](../PersonalDetailsApi.http), Swagger, or the React app, and watch the API console. Each SQL statement EF generates is logged, so you can see the LINQ in `PersonRepository` turn into SQL. Typing in the list page's filters and pressing **Apply** shows how each filter adds a `WHERE` clause.

---

## 14. Glossary

| Term | Meaning here |
|---|---|
| **Aggregate root** | The main entity (`Person`) that owns its children. Children are always loaded and saved through it. |
| **DTO** | Data Transfer Object: a class shaped for the API, not the database (`Contracts/`). |
| **Entity** | A class EF Core maps to a table (`Domain/Entities/`). |
| **DbContext** | EF Core's session with the database. It tracks changes and turns `SaveChanges` into SQL. |
| **Migration** | Generated C# code that changes the schema from one version to the next. |
| **Fluent API** | Configuring EF with C# method calls (`builder.Property(...).HasMaxLength(100)`) instead of attributes. |
| **Tracking / AsNoTracking** | Whether EF remembers loaded entities so it can detect changes. Turn it off for read-only queries. |
| **Global query filter** | A `WHERE` clause EF adds to every query for an entity (`!IsDeleted`). |
| **Soft delete** | Marking a row as deleted instead of removing it. |
| **Dependency Injection (DI)** | The framework creates objects and passes their dependencies through constructors. |
| **Scoped / Singleton** | DI lifetimes: one instance per HTTP request, or one for the whole app. |
| **Middleware** | A component in the request pipeline (`UseExceptionHandler`, `MapControllers`, ...). |
| **ProblemDetails** | Standard JSON error format (RFC 7807). |
| **Options pattern** | Binding a configuration section to a typed class (`ApiDocumentationOptions`). |
| **Deferred execution** | A LINQ query only runs when its results are enumerated (`ToListAsync`, `CountAsync`). |
| **Projection** | Selecting only the needed columns into a new shape (`Select(p => new PersonStatRow(...))`). |
| **SPA** | Single-page application: one HTML page whose content is redrawn by JavaScript (React) as you navigate. |
| **Vite** | The frontend build tool and dev server. It serves the source with hot reload and bundles it for production. |
| **Dev proxy** | The Vite dev server forwarding `/api/...` requests to the ASP.NET Core app, so both appear to the browser as one origin. |
| **CORS** | Cross-Origin Resource Sharing: browser rules that block a page on one origin from reading responses from another, unless the server allows it. |
| **Origin** | Scheme + host + port, e.g. `http://localhost:5173`. Different port means different origin. |
| **AbortController** | Browser API for cancelling a `fetch`; the client-side counterpart of a `CancellationToken`. |
