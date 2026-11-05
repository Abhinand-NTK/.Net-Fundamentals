using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using PersonalDetailsApi.Common.Errors;
using PersonalDetailsApi.Data;
using PersonalDetailsApi.Repositories;
using PersonalDetailsApi.Services;

namespace PersonalDetailsApi.Configuration;

/// <summary>
/// Groups service registrations so Program.cs stays short and readable.
/// Shows the three DI lifetimes: Singleton, Scoped and (implicitly via EF) Scoped DbContext.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PersonalDetails")
            ?? throw new InvalidOperationException("Connection string 'PersonalDetails' is missing.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);                  // one instance for the app's lifetime
        services.AddScoped<IPersonRepository, PersonRepository>();   // one instance per HTTP request
        services.AddScoped<IPersonService, PersonService>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");
        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiDocumentationOptions>()
            .Bind(configuration.GetSection(ApiDocumentationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var docs = configuration.GetSection(ApiDocumentationOptions.SectionName).Get<ApiDocumentationOptions>() ?? new();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(docs.Version, new OpenApiInfo
            {
                Title = docs.Title,
                Version = docs.Version,
                Description = docs.Description,
                Contact = docs.ContactName is null ? null : new OpenApiContact
                {
                    Name = docs.ContactName,
                    Url = docs.ContactUrl is null ? null : new Uri(docs.ContactUrl),
                },
            });

            // XML doc comments (///) become operation summaries, descriptions and examples in Swagger.
            var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xmlFile)) options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);

            options.SupportNonNullableReferenceTypes();
            options.UseAllOfToExtendReferenceSchemas();
        });

        return services;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        var docs = app.Services.GetRequiredService<IOptions<ApiDocumentationOptions>>().Value;
        if (!docs.Enabled) return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{docs.Version}/swagger.json", $"{docs.Title} {docs.Version}");
            options.RoutePrefix = string.Empty; // Swagger UI at the site root
            options.DocumentTitle = docs.Title;
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.EnableTryItOutByDefault();
        });
        return app;
    }
}
