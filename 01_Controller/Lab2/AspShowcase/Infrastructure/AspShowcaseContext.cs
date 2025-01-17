using AspShowcase.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AspShowcase.Infrastructure
{
    public class AspShowcaseContext : DbContext
    {
        public AspShowcaseContext(DbContextOptions opt) : base(opt)
        {
        }

        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Models.Task> Tasks => Set<Models.Task>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Handin> Handins => Set<Handin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Handin>().HasIndex("StudentId", "TaskId").IsUnique();
            // Generic config for all entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // ON DELETE RESTRICT instead of ON DELETE CASCADE
                foreach (var key in entityType.GetForeignKeys())
                    key.DeleteBehavior = DeleteBehavior.Restrict;

                foreach (var prop in entityType.GetDeclaredProperties())
                {
                    // Define Guid as alternate key. The database can create a guid fou you.
                    if (prop.Name == "Guid")
                    {
                        modelBuilder.Entity(entityType.ClrType).HasAlternateKey("Guid");
                        prop.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                    }
                    // Default MaxLength of string Properties is 255.
                    if (prop.ClrType == typeof(string) && prop.GetMaxLength() is null) prop.SetMaxLength(255);
                    // Seconds with 3 fractional digits.
                    if (prop.ClrType == typeof(DateTime)) prop.SetPrecision(3);
                    if (prop.ClrType == typeof(DateTime?)) prop.SetPrecision(3);
                }
            }
        }

        public void Seed()
        {
            Randomizer.Seed = new Random(1804);
            int studentNr = 1000;
            var students = new Faker<Student>("de").CustomInstantiator(f =>
            {
                var lastname = f.Name.LastName();
                var email = $"{(lastname.Length < 3 ? lastname : lastname.Substring(0, 3)).ToLower()}{studentNr}@spengergasse.at";
                return new Student(
                    studentNr: studentNr++, firstname: f.Name.FirstName(),
                    lastname: lastname, email: email)
                { Guid = f.Random.Guid() };
            })
            .Generate(20)
            .ToList();
            Students.AddRange(students);
            SaveChanges();

            var subjects = new string[] { "POS", "DBI", "BWM" };
            var departments = new string[] { "AIF", "BIF", "CIF", "KIF" };

            var teams = new Faker<Team>("de").CustomInstantiator(f =>
            {
                var department = f.Random.ListItem(departments);
                var term = f.Random.Int(3, department == "AIF" || department == "KIF" ? 6 : 8);
                var @class = $"{term}{f.Random.String2(1, "ABC")}{department}";
                // 6BKIF_POS
                var name = $"{@class}_{f.Random.ListItem(subjects)}";
                return new Team(name: name, schoolclass: @class)
                { Guid = f.Random.Guid() };
            })
            .Generate(20)
            .DistinctBy(t => t.Name)
            .ToList();

            Teams.AddRange(teams);
            SaveChanges();

            int i = 1000;
            var teachers = new Faker<Teacher>("de").CustomInstantiator(f =>
            {
                var name = f.Name.LastName();
                var email = $"{name.ToLower()}{i++}@spengergasse.at";
                return new Teacher(
                    firstname: f.Name.FirstName(), lastname: name,
                    email: email)
                { Guid = f.Random.Guid() };
            })
            .Generate(20)
            .ToList();
            Teachers.AddRange(teachers);
            SaveChanges();

            var tasks = new Faker<Task>("de").CustomInstantiator(f =>
            {
                return new Task(
                    subject: f.Random.ListItem(subjects),
                    title: f.Lorem.Sentence(4),
                    team: f.Random.ListItem(teams),
                    teacher: f.Random.ListItem(teachers),
                    expirationDate: f.Date.Between(new DateTime(2022, 9, 1), new DateTime(2023, 6, 30)).Date.AddHours(f.Random.Int(8, 16)),
                    maxPoints: f.Random.Int(16, 32).OrNull(f, 0.4f))
                { Guid = f.Random.Guid() };
            })
            .Generate(200)  // 1 Team soll im Mittel 10 Tasks haben --> 20 Teams x 10 = 200
            .ToList();

            Tasks.AddRange(tasks);
            SaveChanges();

            var handins = new Faker<Handin>("de").CustomInstantiator(f =>
            {
                var task = f.Random.ListItem(tasks);
                return new Handin(student: f.Random.ListItem(students),
                    task: task,
                    created: new DateTime(f.Date.Between(new DateTime(2022, 9, 1), task.ExpirationDate).Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond),
                    description: f.Lorem.Sentence(4),
                    documentUrl: f.Image.PicsumUrl())
                { Guid = f.Random.Guid() };
            })
            .Generate(100)
            .DistinctBy(h => new { h.Student, h.Task })
            .ToList();
            Handins.AddRange(handins);
            SaveChanges();
        }
    }
}