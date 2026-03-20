using Bogus;
using Microsoft.EntityFrameworkCore;
using Languageweek.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Languageweek.Application.Infrastructure;

public class LanguageweekContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Schoolclass> Schoolclasses => Set<Schoolclass>();
    public DbSet<Model.LanguageWeek> Languageweeks => Set<LanguageWeek>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Destination> Destinations => Set<Destination>();

    public LanguageweekContext(DbContextOptions options) : base(options)
    { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LanguageWeek>().HasOne(l => l.Teacher)
            .WithMany(t => t.LanguageweekTeachers);
        modelBuilder.Entity<LanguageWeek>().HasOne(l => l.SupportTeacher)
            .WithMany(t => t.LanguageweekSupportTeachers);
        modelBuilder.Entity<Student>().Property(s => s.Gender).HasConversion<string>();
        modelBuilder.Entity<Teacher>().Property(t => t.Gender).HasConversion<string>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Tabellennamen basieren auf Entity-Namen (nicht DbSet) und starten klein
            var clrName = entityType.ClrType.Name;
            var tableName = char.ToLowerInvariant(clrName[0]) + clrName[1..];
            entityType.SetTableName(tableName);

            foreach (var property in entityType.GetProperties())
            {
                var propName = property.Name;
                // Spaltenname: Nur erster Buchstabe klein
                property.SetColumnName(char.ToLowerInvariant(propName[0]) + propName[1..]);

                // Standardkonfigurationen
                if (property.ClrType == typeof(string) && property.GetMaxLength() is null)
                    property.SetMaxLength(255);

                if (property.ClrType == typeof(DateTime)) property.SetPrecision(3);
                if (property.ClrType == typeof(DateTime?)) property.SetPrecision(3);
            }

            // FK-Verhalten
            foreach (var fk in entityType.GetForeignKeys())
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    public void Seed()
    {
        Randomizer.Seed = new Random(1832);
        var minDate = new DateTime(2025, 9, 1);
        var maxDate = new DateTime(2026, 6, 1);
        var faker = new Faker();

        // Generate a list of 5 Teachers without Bogus
        var teachers = GenerateDistinct(f =>
        {
            var lastname = f.Name.LastName();
            var firstname = f.Name.FirstName();
            var shortname = $"{lastname[..2]}{firstname[0]}".ToUpper();
            return new Teacher(
                shortname, firstname, lastname,
                $"{lastname.ToLower()}@spengergasse.at", f.Random.Enum<Gender>());
        }, 10, t => t.Shortname);

        List<Schoolclass> schoolclasses =
        [
            new Schoolclass("4AHIF", "HIF"),
            new Schoolclass("4BHIF", "HIF"),
            new Schoolclass("4CHIF", "HIF"),
            new Schoolclass("4AHBGM", "HBGM"),
            new Schoolclass("6AAIF", "IF"),
        ];
        Schoolclasses.AddRange(schoolclasses);
        SaveChanges();

        List<Destination> destinations =
        [
            new Destination("London", "Großbritannien"),
            new Destination("Dublin", "Irland"),
            new Destination("Valetta", "Malta"),
            new Destination("Edinburgh", "Großbritannien"),
            new Destination("Galway", "Irland")
        ];
        Destinations.AddRange(destinations);
        SaveChanges();

        var onlyMaleClasses = new Dictionary<string, bool>
        {
            { "4AHIF", false },
            { "4BHIF", true },
            { "4AHBGM", false },
            { "4CHIF", true },
            { "6AAIF", true },
        };

        var students = GenerateDistinct(f =>
        {
            var schoolclass = f.Random.ListItem(schoolclasses);
            var onlyMaleClass = onlyMaleClasses[schoolclass.Shortname];
            var gender = onlyMaleClass ? Gender.Male : f.Random.Enum<Gender>();
            var lastname = f.Name.LastName();
            var firstname = f.Name.FirstName(gender == Gender.Male
                ? Bogus.DataSets.Name.Gender.Male
                : Bogus.DataSets.Name.Gender.Female);
            var email = $"{firstname.ToLower()}.{lastname.ToLower()}@spengergasse.at";
            return new Student(
                firstname, lastname, email, schoolclass,
                gender,
                f.Date.BetweenDateOnly(new DateOnly(2008, 1, 1), new DateOnly(2009, 1, 1)));
        }, schoolclasses.Count * 25, s => s.Email)
        .GroupBy(s => s.Schoolclass)
        .ToDictionary(g => g.Key, g => g.ToList());

        int teacherIdx = 0;
        var languageweeks = schoolclasses[..^1].Select(schoolclass =>
        {
            var destination = faker.Random.ListItem(destinations);
            var from = faker.Date.BetweenDateOnly(
                DateOnly.FromDateTime(minDate), DateOnly.FromDateTime(maxDate));
            var to = from.AddDays(faker.Random.Int(5, 8));
            var teacher = teachers[teacherIdx++];
            var supportTeacher = teachers[teacherIdx++].OrNull(faker, 0.2f);
            var pricePerPerson = faker.Random.Int(800, 1200);
            var languageweek = new LanguageWeek(
                schoolclass, destination, from, to, teacher, pricePerPerson)
            { SupportTeacher = supportTeacher };
            return languageweek;
        })
        .ToList();
        Languageweeks.AddRange(languageweeks);
        SaveChanges();
        languageweeks[..^1].ForEach(l => GenerateDistinct(f =>
        {
            var student = f.Random.ListItem(students[l.Schoolclass]);
            var registrationDate = l.From
                .AddDays(f.Random.Int(-90, -70))
                .ToDateTime(new TimeOnly(8, 0, 0).AddMinutes(f.Random.Int(0, 8 * 60)));
            return new Registration(l, student, registrationDate);
        },
            faker.Random.Int(6 * students[l.Schoolclass].Count / 10, 8 * students[l.Schoolclass].Count / 10),
            r => r.Student));
    }

    private List<T> Generate<T>(Func<Faker, T> generator, int count) where T : class
    {
        var data = new Faker<T>("de")
            .CustomInstantiator(generator)
            .Generate(count)
            .ToList();
        var set = Set<T>();
        set.AddRange(data);
        SaveChanges();
        return data;
    }

    private List<T> GenerateDistinct<T, Tkey>(Func<Faker, T> generator, int count, params Func<T, Tkey>[] distinctBys) where T : class
    {
        var dataEnumerable = new Faker<T>("de")
            .CustomInstantiator(generator)
            .GenerateForever();
        foreach (var distinctBy in distinctBys)
            dataEnumerable = dataEnumerable.DistinctBy(distinctBy);
        var data = dataEnumerable.Take(count).ToList();
        var set = Set<T>();
        set.AddRange(data);
        SaveChanges();
        return data;
    }
}