using FastEndpoints;
using FastEndpoints.Swagger;
using MamisSolidarias.Infrastructure.Donors;
using Microsoft.EntityFrameworkCore;

namespace MamisSolidarias.WebAPI.Donors.StartUp;

internal static class MiddlewareRegistrator
{
    public static void Register(WebApplication app)
    {
        app.UseDefaultExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints();
        app.MapGraphQL();
        
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DonorsDbContext>();
            db.Database.Migrate();
        }

        if (!app.Environment.IsProduction())
        {
            app.UseOpenApi();
            app.UseSwaggerUi3(t => t.ConfigureDefaults());
        }
    }
}