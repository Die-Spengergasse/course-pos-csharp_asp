using Languageweek.Application.Infrastructure;
using Languageweek.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
public class Program
{
    private static void Main(string[] args)
    {
        // STEP 1: Configuring ASP.NET Core Services
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        // Configure Datebase with settings from appsettings.json (section ConnectionStrings)
        var databaseName = builder.Configuration.GetConnectionString("Default")
            ?? throw new Exception("Missing ConnectionString:Default in appsettings.json.");
        var connection = new SqliteConnection(databaseName);
        builder.Services.AddDbContextFactory<LanguageweekContext>(options =>
            options.UseSqlite(connection));

        builder.Services.AddSingleton<TimeProvider>((provider) => TimeProvider.System);

        builder.Services.AddScoped<IRegistrationService, RegistrationService>();

        // STEP 2: Configuring ASP.NET Core request pipeline
        var app = builder.Build();
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            // Map http://localhost:5080/openapi/v1.json
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "API v1");
            });
            using (var scope = app.Services.CreateScope())
            using (var service = scope.ServiceProvider.GetRequiredService<LanguageweekContext>())
            {
                service.Database.EnsureDeleted();
                // To prevent EnsureCreated from closing the connection and losing the in-memory DB.
                connection.Open();
                service.Database.EnsureCreated();
                if (app.Environment.IsDevelopment()) service.Seed();
            }
        }
        // Assign controllers to routes.
        app.MapControllers();
        app.Run();
        app.Logger.LogInformation($"Close connection to {databaseName}...");
        connection.Close();
    }
}

