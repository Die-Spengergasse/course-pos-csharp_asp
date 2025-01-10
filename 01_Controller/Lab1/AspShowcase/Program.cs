using AspShowcase.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        // ***********************************************************
        var builder = WebApplication.CreateBuilder(args);
        // Sucht nach Klassen, die die Annotation ApiController haben.
        builder.Services.AddControllers();
        // Der Service Provider registriert alle Typen, die Sie nachher
        // als Parameter im Konstruktor (z. B. von PersonsController) verwenden
        // können.
        // builder.Configuration holt sich u. a. von appsettings.json
        // die entsprechenden Werte.
        //     builder.Configuration["ReplyTo"] -> Liest den Key ReplyTo aus appsettings.json
        //     builder.Configuration.GetConnectionString("Default")
        //         -> Liest den Key Default aus dem Key ConnectionStrings
        builder.Services.AddDbContext<AspShowcaseContext>(opt => 
            opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        // API Explorer
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        // ************************************************************
        // Middleware
        // Schaltet Features erst ein
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            using (var scope = app.Services.CreateScope())
                using (var db = scope.ServiceProvider.GetRequiredService<AspShowcaseContext>())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.Seed();
            }
        }
        // Leitet alle Requests, für die Controller existieren, weiter.
        app.MapControllers();
        // Liefert Dateien in wwwroot aus.
        app.UseStaticFiles();
        app.Run();
    }
}