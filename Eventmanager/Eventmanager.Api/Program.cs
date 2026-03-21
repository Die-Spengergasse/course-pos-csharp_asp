using Eventmanager.Api.Endpoints;
using Eventmanager.Application.Model;
using Eventmanager.Application.Repositories;
using Eventmanager.Application.Services;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using IdHasher;
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
        builder.Services.AddControllers()
            .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new IdConverter()));

        // Configure Datebase with settings from appsettings.json (section ConnectionStrings)
        var databaseName = builder.Configuration.GetConnectionString("Default")
            ?? throw new Exception("Missing ConnectionString:Default in appsettings.json.");
        var connection = new SqliteConnection(databaseName);
        builder.Services.AddDbContextFactory<EventContext>(options =>
            options.UseSqlite(connection));

        builder.Services.AddSingleton<TimeProvider>((provider) => TimeProvider.System);

        builder.Services.AddScoped<IEventService, EventService>((provider) => new EventService(
            db: provider.GetRequiredService<EventContext>(),
            timeProvider: provider.GetRequiredService<TimeProvider>(),
            isDevelopment: builder.Environment.IsDevelopment()));

        builder.Services.AddScoped<IRepository<Show>, Repository<Show>>((provider) =>
            new Repository<Show>(db: provider.GetRequiredService<EventContext>()));

        Id.Secret = Convert.FromBase64String(
            builder.Configuration["IdEncoderSecret"] ?? throw new Exception("Missing IdEncoderSecret."));

        builder.Services.AddGraphQLServer()
                    .AddQueryType<Eventmanager.Application.GraphQL.Query>()
                    .AddType<Entity>()
                    .AddProjections()
                    .AddFiltering()
                    .AddSorting()
                    .RegisterDbContextFactory<EventContext>();

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
            using (var service = scope.ServiceProvider.GetRequiredService<EventContext>())
            {
                service.Database.EnsureDeleted();
                // To prevent EnsureCreated from closing the connection and losing the in-memory DB.
                service.Database.OpenConnection();
                service.Database.EnsureCreated();
                if (app.Environment.IsDevelopment()) service.Seed();
            }
        }
        // Assign controllers to routes.
        app.MapControllers();
        app.MapContingentEndpoints();
        app.MapGraphQL();         // http://localhost:5080/graphql
        app.Run();
        app.Logger.LogInformation($"Close connection to {databaseName}...");
        connection.Close();
    }
}

