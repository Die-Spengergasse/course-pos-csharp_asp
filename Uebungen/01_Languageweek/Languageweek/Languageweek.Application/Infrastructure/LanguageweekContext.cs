using Bogus;
using Microsoft.EntityFrameworkCore;
using Languageweek.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Languageweek.Application.Infrastructure
{
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

        public List<Teacher> GetTeachersWithMinCountOfParticipations(int count)
        {
            // TODO: Add your implementation
            throw new NotImplementedException();
        }

        public List<Schoolclass> GetClassesWithoutLanguageWeek()
        {
            // TODO: Add your implementation
            throw new NotImplementedException();
        }
        public record SchoolclassStatistics(int Id, string Shortname, int MaleCount, int FemaleCount);
        /// </summary>
        public List<SchoolclassStatistics> CalcSchoolclassStatistics()
        {
            // TODO: Add your implementation
            throw new NotImplementedException();
        }

        public record LanguageWeekRegistrationRate(
            int Id, DateOnly From, DateOnly To, decimal TotalPrice,
            int DestinationId, string DestinationCity, string DestinationCountry,
            int SchoolclassId, string SchoolclassShortname, decimal Percentage);
        public List<LanguageWeekRegistrationRate> CalcRegistrationRates()
        {
            // TODO: Add your implementation
            throw new NotImplementedException();
        }

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
            var teachers = new List<Teacher>
            {
                new Teacher("WIS", "Stefanie", "Williams", "williams@spengergasse.at", Gender.Female),
                new Teacher("JOS", "Susan", "Johnson", "johnson@spengergasse.at", Gender.Female),
                new Teacher("BRM", "Michael", "Brown", "brown@spengergasse.at", Gender.Male),
                new Teacher("SMM", "Martin", "Smith", "smith@spengergasse.at", Gender.Male),
                new Teacher("JOM", "Manfred", "Jones", "jones@spengergasse.at", Gender.Male),
            };
            Teachers.AddRange(teachers);
            SaveChanges();

            var schoolclasses = new List<Schoolclass>
            {
                new Schoolclass("4AHIF", "HIF"),
                new Schoolclass("4BHIF", "HIF"),
                new Schoolclass("4CHIF", "HIF"),
                new Schoolclass("4AHBGM", "HBGM")
            };
            Schoolclasses.AddRange(schoolclasses);
            SaveChanges();

            var destinations = new List<Destination>
            {
                new Destination("London", "Großbritannien"),
                new Destination("Dublin", "Irland"),
                new Destination("Valetta", "Malta"),
                new Destination("Edinburgh", "Großbritannien"),
                new Destination("Galway", "Irland")
            };
            Destinations.AddRange(destinations);
            SaveChanges();

            var onlyMaleClasses = new Dictionary<string, bool>
            {
                { "4AHIF", false },
                { "4BHIF", true },
                { "4CHIF", true },
                { "4AHBGM", false }
            };

            var students = new Faker<Student>("de").CustomInstantiator(f =>
            {
                var schoolclass = f.Random.ListItem(schoolclasses);
                var onlyMaleClass = onlyMaleClasses[schoolclass.Shortname];
                var gender = onlyMaleClass ? Gender.Male : f.Random.Enum<Gender>();
                var lastname = f.Name.LastName();
                var firstname = f.Name.FirstName(gender == Gender.Male ? Bogus.DataSets.Name.Gender.Male : Bogus.DataSets.Name.Gender.Female);
                var email = $"{firstname.ToLower()}.{lastname.ToLower()}@spengergasse.at";
                return new Student(
                    firstname, lastname, email, schoolclass,
                    gender,
                    f.Date.BetweenDateOnly(new DateOnly(2008, 1, 1), new DateOnly(2009, 1, 1)));
            })
            .Generate(4 * 25)
            .DistinctBy(s => s.Email)
            .ToList();
            Students.AddRange(students);
            SaveChanges();

            {
                var languageweek = new Model.LanguageWeek(
                    schoolclasses.First(s => s.Shortname == "4AHIF"),
                    destinations[0],
                    new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 9),
                    teachers[0],
                    800);
                languageweek.SupportTeacher = teachers[2];
                languageweek.Registrations.AddRange(GenerateRegistrations(faker, students, languageweek));
                Languageweeks.Add(languageweek);
            }
            {
                var languageweek = new Model.LanguageWeek(
                    schoolclasses.First(s => s.Shortname == "4CHIF"),
                    destinations[0],
                    new DateOnly(2026, 4, 8), new DateOnly(2026, 4, 15),
                    teachers[1],
                    1200);
                languageweek.SupportTeacher = teachers[2];
                languageweek.Registrations.AddRange(GenerateRegistrations(faker, students, languageweek));
                Languageweeks.Add(languageweek);
            }
            {
                var languageweek = new Model.LanguageWeek(
                    schoolclasses.First(s => s.Shortname == "4AHBGM"),
                    destinations[1],
                    new DateOnly(2026, 6, 8), new DateOnly(2026, 6, 16),
                    teachers[2],
                    1200);
                languageweek.SupportTeacher = teachers[1];
                languageweek.Registrations.AddRange(GenerateRegistrations(faker, students, languageweek));
                Languageweeks.Add(languageweek);
            }
            SaveChanges();
        }

        private List<Registration> GenerateRegistrations(Faker f, List<Student> students, Model.LanguageWeek languageweek)
        {
            var className = languageweek.Schoolclass.Shortname;
            var classStudents = students.Where(s => s.Schoolclass.Shortname == className).ToList();
            var count = f.Random.Int((int)Math.Ceiling(classStudents.Count * 0.7), classStudents.Count);
            var registrations = classStudents.Select(cs =>
            {
                var registrationDate = f.Date.Between(
                    languageweek.From.AddDays(-60).ToDateTime(new TimeOnly(0)),
                    languageweek.From.AddDays(-30).ToDateTime(new TimeOnly(0)));
                registrationDate = new DateTime(registrationDate.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
                return new Registration(languageweek, cs, registrationDate);
            }).ToList();

            return f.Random.ListItems(registrations, count).ToList();
        }
    }
}