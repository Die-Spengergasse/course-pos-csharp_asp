using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Fitnesscenter.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fitnesscenter.Infrastructure
{
    public class FitnessContext : DbContext
    {
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Visit> Visits => Set<Visit>();
        public DbSet<Trainer> Trainers => Set<Trainer>();
        public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
        public DbSet<Participation> Participations => Set<Participation>();
        public DbSet<Room> Rooms => Set<Room>();

        public FitnessContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
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
            Randomizer.Seed = new Random(837);
            var faker = new Faker("de");

            // Räume
            var rooms = new[]
            {
                new Room("Kraft 1", "Kraft"),
                new Room("Cardio 2", "Cardio"),
                new Room("Yoga-Raum", "Yoga")
            };
            Rooms.AddRange(rooms);
            SaveChanges();

            // Mitglieder
            var members = new[]
            {
                new Member("Alice", "Example", "alice@example.com", "Premium", new DateTime(2023, 1, 1), true),
                new Member("Bob", "Fit", "bob@example.com", "Basic", new DateTime(2024, 3, 15), true),
                new Member("Charlie", "Chill", "charlie@example.com", "Basic", null, false),
                new Member("Dora", "Power", "dora@example.com", "Premium", new DateTime(2023, 2, 20), true),
                new Member("Eli", "Motion", "eli@example.com", "Basic", new DateTime(2023, 9, 5), true)
            };
            Members.AddRange(members);
            SaveChanges();

            var membersForParticipation = members.Take(4).ToList();
            // Trainer
            var trainers = new[]
            {
                new Trainer("Lisa", "Strong", "Krafttraining", false),
                new Trainer("Tom", "Cardio", "Ausdauer", true),
                new Trainer("Sara", "Zen", "Yoga", false)
            };
            Trainers.AddRange(trainers);
            SaveChanges();

            // TrainingSessions + Participations
            var sessions = new List<TrainingSession>();
            var participations = new List<Participation>();
            foreach (var trainer in trainers)
            {
                var trainerSessions = new Faker<TrainingSession>("de")
                    .CustomInstantiator(f =>
                    {
                        var time = f.Date.Between(new DateTime(2025, 1, 1), new DateTime(2025, 6, 30));
                        time = new DateTime(time.Ticks / TimeSpan.TicksPerMinute / 5 * TimeSpan.TicksPerMinute * 5);
                        var room = f.Random.ListItem(rooms);

                        var session = new TrainingSession(
                            trainer, room, time,
                            type: trainer.Specialization,
                            durationMinutes: f.Random.Int(30, 90) / 5 * 5,
                            maxParticipants: f.Random.Int(2, 5)
                        );

                        var sessionParticipants = f.Random.ListItems(
                            membersForParticipation, f.Random.Int(2, Math.Min(4, session.MaxParticipants))).ToList();
                        foreach (var member in sessionParticipants)
                        {
                            int? rating = f.Random.Bool(0.8f) ? f.Random.Int(1, 5) : null;
                            participations.Add(new Participation(session, member, rating));
                        }

                        sessions.Add(session);
                        return session;
                    })
                    .Generate(faker.Random.Int(3, 6));
            }

            TrainingSessions.AddRange(sessions);
            SaveChanges();

            Participations.AddRange(participations);
            SaveChanges();

            // Visits (raumlos)
            foreach (var member in members)
            {
                var visits = new Faker<Visit>("de")
                    .CustomInstantiator(f =>
                    {
                        var start = f.Date.Between(new DateTime(2025, 1, 1), new DateTime(2025, 6, 30));
                        start = new DateTime(start.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
                        DateTime? end = start.AddMinutes(f.Random.Int(30, 120));
                        end = (new DateTime(end.Value.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond)).OrNull(f, 0.3f);
                        return new Visit(member, start, end);
                    })
                    .Generate(faker.Random.Int(5, 10));
                Visits.AddRange(visits);
            }

            SaveChanges();
        }
    }
}
