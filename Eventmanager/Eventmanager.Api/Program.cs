using Eventmanager.Application.Model;
using Eventmanager.Application.Services;
using Eventmanager.Infrastructure;
using IdHasher;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        //builder.Services.AddDbContext<EventContext>(options =>
        //    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddDbContextFactory<EventContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddSingleton<TimeProvider>((provider) => TimeProvider.System);

        builder.Services.AddScoped<IEventService, EventService>((provider) => new EventService(
            db: provider.GetRequiredService<EventContext>(),
            timeProvider: provider.GetRequiredService<TimeProvider>(),
            isDevelopment: builder.Environment.IsDevelopment()));

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
                service.Database.EnsureCreated();
                if (app.Environment.IsDevelopment()) service.Seed();
            }
        }
        // Assign controllers to routes.
        app.MapControllers();
        app.MapGraphQL();         // http://localhost:5080/graphql
        app.Run();
    }
}

