using EntityFramework.Exceptions.PostgreSQL;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using HotChocolate.Diagnostics;
using MamisSolidarias.Infrastructure.Donors;
using MamisSolidarias.Utils.Security;
using MamisSolidarias.WebAPI.Donors.Extensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MamisSolidarias.WebAPI.Donors.StartUp;

internal static class ServiceRegistrator
{
    public static void Register(WebApplicationBuilder builder)
    {
        var connectionString = builder.Environment.EnvironmentName.ToLower() switch
        {
            "production" => builder.Configuration.GetConnectionString("Production"),
            _ => builder.Configuration.GetConnectionString("Development")
        };

        builder.Services.AddOpenTelemetry(builder.Configuration, builder.Logging);
        
        builder.Services.AddFastEndpoints(t => t.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);
        builder.Services.AddAuthenticationJWTBearer(
            builder.Configuration["Jwt:Key"],
            builder.Configuration["Jwt:Issuer"]
        );

        builder.Services.AddAuthorization(t => t.ConfigurePolicies(Services.Donors));
        
        builder.Services.AddDbContext<DonorsDbContext>(
            t =>
            {
                t.UseNpgsql(connectionString, r => r.MigrationsAssembly("MamisSolidarias.WebAPI.Donors"))
                    .EnableSensitiveDataLogging(!builder.Environment.IsProduction())
                    .EnableDetailedErrors(!builder.Environment.IsProduction());
                t.UseExceptionProcessor();
            }
        );

        builder.Services.AddGraphQLServer()
            .AddQueryType<Queries.Donors>()
            .AddInstrumentation(t =>
            {
                t.Scopes = ActivityScopes.All;
                t.IncludeDocument = true;
                t.RequestDetails = RequestDetails.All;
                t.IncludeDataLoaderKeys = true;
            })
            .AddAuthorization()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .RegisterDbContext<DonorsDbContext>()
            .PublishSchemaDefinition(t =>
                t.SetName($"{Services.Donors}gql")
                    .AddTypeExtensionsFromFile("./Stitching.graphql")
            );

        if (!builder.Environment.IsProduction())
            builder.Services.AddSwaggerDoc(t => t.Title = "Donors");
    }
}