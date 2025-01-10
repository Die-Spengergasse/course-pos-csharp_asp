// =================================================================================================
// Vordefinierte Tests für das LanguageweekService.
// Sie werden zur Korrektur verwendet, hier darf nichts verändert werden.
// =================================================================================================
using Languageweek.Application.Commands;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Model;
using Languageweek.Application.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Languageweek.Test;

public class LanguageweekServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly LanguageweekContext _db;
    private readonly FakeTimeProvider _timeProvider;
    private readonly LanguageweekService _service;

    public LanguageweekServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");

        _db = new LanguageweekContext(new DbContextOptionsBuilder().UseSqlite(_conn).Options);
        _db.Database.EnsureDeleted();
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        // Wir fixieren die Zeit auf den 15. März 2026
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(new DateTime(2026, 3, 15, 12, 0, 0)));
        _service = new LanguageweekService(db: _db, timeProvider: _timeProvider);
    }

    private Tentity[] AddEntities<Tentity>(params Tentity[] items) where Tentity : class
    {
        _db.Set<Tentity>().AddRange(items);
        _db.SaveChanges();
        return items;
    }

    private void GenerateFixtures()
    {
        var classes = AddEntities(
            new Schoolclass("4AHIF", "HIF") { Id = 1 },
            new Schoolclass("4BHIF", "HIF") { Id = 2 },
            new Schoolclass("4CHIF", "HIF") { Id = 3 },
            new Schoolclass("4DHIF", "HIF") { Id = 4 }
        );

        var destinations = AddEntities(
            new Destination("London", "Großbritannien") { Id = 1 }
        );

        var teachers = AddEntities(
            new Teacher("TEA", "Tom", "Teacher", "tea@spengergasse.at", Gender.Male) { Id = 1 },
            new Teacher("SMI", "Sarah", "Smith", "smi@spengergasse.at", Gender.Female) { Id = 2 },
            new Teacher("BRM", "Michael", "Brown", "brown@spengergasse.at", Gender.Male) { Id = 3 }
        );

        var languageweeks = AddEntities(
            // LW 1: Zukunft, keine Anmeldungen -> Darf gelöscht und bearbeitet werden. Blockiert Klasse 1.
            new Application.Model.LanguageWeek(classes[0], destinations[0], new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 8), teachers[0], 800) { Id = 1 },

            // LW 2: Vergangenheit (Februar 2026). Darf NICHT gelöscht werden! Blockiert Klasse 3.
            new Application.Model.LanguageWeek(classes[2], destinations[0], new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 8), teachers[1], 800) { Id = 2 },

            // LW 3: Zukunft, ABER mit angemeldeten Schülern. Darf nur mit forceDelete gelöscht werden. Blockiert Klasse 4.
            new Application.Model.LanguageWeek(classes[3], destinations[0], new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 8), teachers[2], 800) { Id = 3 }
        );

        var students = AddEntities(
            new Student("Leo", "Löwe", "leo@test.at", classes[3], Gender.Male, new DateOnly(2008, 3, 3)) { Id = 1 }
        );

        AddEntities(
            new Registration(languageweeks[2], students[0], new DateTime(2026, 3, 10)) { Id = 1 }
        );
    }

    [Fact]
    public async Task T01_CreateLanguageWeekCreatesNewLanguageWeekTest()
    {
        GenerateFixtures();
        var cmd = new CreateLanguageWeekCommand(
            2, 1, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 8), 2, 900);
        await _service.CreateLanguageWeekAsync(cmd);

        _db.ChangeTracker.Clear();
        Assert.True(_db.Languageweeks.Any(l => l.Schoolclass.Id == 2 && l.PricePerPerson == 900));
    }

    [Theory]
    // Klasse 1 hat bereits LW 1, daher wird eine neue Woche verweigert (egal welches Datum, da Regel vereinfacht)
    [InlineData(1, 1, "2026-07-01", "2026-07-08", 2, "LanguageweekServiceException", "Klasse hat bereits eine Sprachwoche geplant")]
    // Klasse 2 ist frei, aber Lehrer 1 ist in diesem Zeitraum schon bei LW 1 (01.05. - 08.05.)
    [InlineData(2, 1, "2026-05-02", "2026-05-06", 1, "LanguageweekServiceException", "Lehrer ist in diesem Zeitraum bereits auf einer anderen Sprachwoche")]
    public async Task T02_CreateLanguageWeekThrowsExceptionWithMessageTest(
        int classId, int destId, string fromString, string toString, int teacherId,
        string expectedErrorType, string expectedMessage)
    {
        GenerateFixtures();

        var cmd = new CreateLanguageWeekCommand(
            classId, destId, DateOnly.Parse(fromString), DateOnly.Parse(toString), teacherId, 800);

        var ex = await Assert.ThrowsAnyAsync<LanguageweekServiceException>(() =>
            _service.CreateLanguageWeekAsync(cmd));

        Assert.Equal(expectedErrorType, ex.GetType().Name);
        Assert.Contains(expectedMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task T03_UpdateLanguageWeekUpdatesDataTest()
    {
        GenerateFixtures();

        var cmd = new UpdateLanguageWeekCommand(1, 1, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 8), 2, 850);
        await _service.UpdateLanguageWeekAsync(cmd);

        _db.ChangeTracker.Clear();
        var updatedLw = _db.Languageweeks.Include(l => l.Teacher).First(l => l.Id == 1);
        Assert.Equal(850, updatedLw.PricePerPerson);
        Assert.Equal(2, updatedLw.Teacher.Id);
    }

    [Fact]
    public async Task T04_UpdateLanguageWeekPriceUpdatesPriceOnlyTest()
    {
        GenerateFixtures();

        await _service.UpdateLanguageWeekPriceAsync(new UpdateLanguageWeekPriceCommand(1, 999));

        _db.ChangeTracker.Clear();
        Assert.Equal(999, _db.Languageweeks.First(l => l.Id == 1).PricePerPerson);
    }

    [Fact]
    public async Task T05_DeleteLanguageWeekDeletesWhenValidTest()
    {
        GenerateFixtures();
        await _service.DeleteLanguageWeekAsync(1);
        _db.ChangeTracker.Clear();
        Assert.False(_db.Languageweeks.Any(l => l.Id == 1));
    }

    [Fact]
    public async Task T06_DeleteLanguageWeekThrowsIfPastTest()
    {
        GenerateFixtures();
        var ex = await Assert.ThrowsAsync<LanguageweekServiceException>(() =>
            _service.DeleteLanguageWeekAsync(2, forceDelete: true)); // Sogar forceDelete hilft hier nicht!
        Assert.Contains("Vergangene Sprachwochen dürfen nicht gelöscht werden", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task T07_DeleteLanguageWeekThrowsIfRegistrationsExistTest()
    {
        GenerateFixtures();
        var ex = await Assert.ThrowsAsync<LanguageweekServiceException>(() =>
            _service.DeleteLanguageWeekAsync(3, forceDelete: false));
        Assert.Contains("bereits Schüler angemeldet", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task T08_DeleteLanguageWeekDeletesWithForceDeleteTest()
    {
        GenerateFixtures();

        // LW 3 hat Registrierungen -> Mit forceDelete = true muss es klappen!
        await _service.DeleteLanguageWeekAsync(3, forceDelete: true);

        _db.ChangeTracker.Clear();
        Assert.False(_db.Languageweeks.Any(l => l.Id == 3));
        Assert.False(_db.Registrations.Any(r => r.Languageweek.Id == 3)); // Registrierungen müssen explizit mitgelöscht worden sein
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
