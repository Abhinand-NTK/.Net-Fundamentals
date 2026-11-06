using System.Text.Json.Serialization;
using PersonalDetailsApi.Configuration;
using PersonalDetailsApi.Data;

// ---------------- 1. Build: register services in the DI container ----------------
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplicationServices()
    .AddFrontendCors(builder.Configuration)
    .AddApiDocumentation(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums travel as text ("Female") rather than numbers; nulls are omitted.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// ---------------- 2. Prepare the database ----------------
await app.InitializeDatabaseAsync();

// ---------------- 3. Configure the HTTP pipeline (order matters) ----------------
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();               // must run before the endpoints so preflight (OPTIONS) requests are answered
app.UseApiDocumentation();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

// Makes the auto-generated Program class visible to integration tests (WebApplicationFactory<Program>).
public partial class Program;
