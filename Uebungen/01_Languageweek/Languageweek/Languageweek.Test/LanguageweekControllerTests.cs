// =================================================================================================
// Vordefinierte Tests für die Endpunkte in LanguageweeksController
// Sie werden zur Korrektur verwendet, hier darf nichts verändert werden.
// =================================================================================================
using AspTestHelpers;
using Languageweek.Application.Commands;
using Languageweek.Application.Dtos;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Languageweek.Test;

public class LanguageweeksControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory<LanguageweekContext> _factory;
    private readonly TimeProvider _timeProvider =
        new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero));
    private readonly SqliteConnection _connection =
        new SqliteConnection("DataSource=:memory:");

    public LanguageweeksControllerTests()
    {
        _factory = new TestWebApplicationFactory<LanguageweekContext>();

        _factory.SubstituteService<LanguageweekContext>(opt => opt.UseSqlite(_connection));
        _factory.SubstituteService<TimeProvider>(provider => _timeProvider, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Creates global test data for all unit tests.
    /// </summary>
    private void GenerateFixtures()
    {
        _factory.InitializeDatabase(db =>
        {
            var class1 = new Schoolclass("4AHIF", "HIF"); // ID 1
            var class2 = new Schoolclass("4BHIF", "HIF"); // ID 2
            var class3 = new Schoolclass("4CHIF", "HIF"); // ID 3
            var class4 = new Schoolclass("4DHIF", "HIF"); // ID 4 (Frei für Tests)

            var dest1 = new Destination("London", "Großbritannien"); // ID 1

            var teacher1 = new Teacher("TEA", "Tom", "Teacher", "tea@spengergasse.at", Gender.Male);       // ID 1
            var teacher2 = new Teacher("SMI", "Sarah", "Smith", "smi@spengergasse.at", Gender.Female);     // ID 2
            var teacher3 = new Teacher("BRM", "Michael", "Brown", "brown@spengergasse.at", Gender.Male);   // ID 3
            var teacher4 = new Teacher("SMM", "Martin", "Smith", "smith@spengergasse.at", Gender.Male);    // ID 4

            var student1 = new Student("Leo", "Löwe", "leo@test.at", class3, Gender.Male, new DateOnly(2008, 3, 3)); // ID 1 (4CHIF)

            // Languageweek 1: Zukunft, keine Anmeldungen (Darf gelöscht/bearbeitet werden)
            var lw1 = new Application.Model.LanguageWeek(class1, dest1, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 8), teacher1, 800);
            // Languageweek 2: Vergangenheit (Darf NICHT gelöscht werden)
            var lw2 = new Application.Model.LanguageWeek(class2, dest1, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 8), teacher2, 800);
            // Languageweek 3: Zukunft, ABER mit Anmeldungen (Braucht forceDelete)
            var lw3 = new Application.Model.LanguageWeek(class3, dest1, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 8), teacher3, 800);

            var registration = new Registration(lw3, student1, new DateTime(2026, 3, 10));

            db.AddRange(class1, class2, class3, class4, dest1, teacher1, teacher2, teacher3, teacher4, student1, lw1, lw2, lw3, registration);
            db.SaveChanges();
        });
    }

    /// <summary>
    /// Prüft, ob der LanguageweeksController nur das Interface als Dependency besitzt.
    /// </summary>
    [Fact]
    public async Task T01_LanguageweeksControllerDependsOnInstanceTest()
    {
        var trainingSessionsControllerType = Assembly.Load("Languageweek.Api").GetType("Languageweek.Api.Controllers.LanguageweeksController");
        Assert.NotNull(trainingSessionsControllerType);
        Assert.True(
            trainingSessionsControllerType.GetConstructors().SingleOrDefault()?.GetParameters().SingleOrDefault()?.ParameterType.Name == "ILanguageweekService",
            "LanguageweeksController must have one (1) constructor with one (1) parameter of type ILanguageweekService.");
    }

    /// <summary>
    /// Prüft, ob GET /api/languageweeks HTTP 200 mit den entsprechenden Daten ohne includeRegistrations liefert.
    /// </summary>
    [Fact]
    public async Task T02_GetGetLanguageWeeksReturns200WithoutRegistrationsTest()
    {
        GenerateFixtures();

        var (response, languageWeek) =
            await _factory.GetHttpContent<List<LanguageWeekWithRegistrationsDto>>("/api/languageweeks");

        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(languageWeek);
        Assert.Equal(3, languageWeek.Count);
        Assert.True(languageWeek.All(l => l.Registrations.Count == 0));
    }

    /// <summary>
    /// Prüft, ob GET /api/languageweeks?includeRegistrations=true HTTP 200 mit den entsprechenden
    /// Daten liefert.
    /// </summary>
    [Fact]
    public async Task T03_GetGetLanguageWeeksReturns200WithRegistrationsTest()
    {
        GenerateFixtures();

        var (response, languageWeek) =
            await _factory.GetHttpContent<List<LanguageWeekWithRegistrationsDto>>("/api/languageweeks?includeRegistrations=true");

        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(languageWeek);
        Assert.Equal(3, languageWeek.Count);
        Assert.Contains(languageWeek, l => l.Registrations.Count > 0);
    }

    /// <summary>
    /// Prüft, ob GET /api/languageweeks/1 HTTP 200 mit den entsprechenden Daten liefert.
    /// </summary>
    [Fact]
    public async Task T04_GetLanguageWeekByIdReturns200WithDataTest()
    {
        GenerateFixtures();

        var (response, languageWeek) = await _factory.GetHttpContent<LanguageWeekDto>("/api/languageweeks/1");

        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(languageWeek);
        Assert.Equal(1, languageWeek.Id);
        Assert.Equal("4AHIF", languageWeek.SchoolclassShortname);
    }

    /// <summary>
    /// Prüft, ob POST /api/languageweeks HTTP 201 mit den entsprechenden Daten liefert.
    /// </summary>
    [Fact]
    public async Task T05_CreateLanguageWeekReturns201Test()
    {
        GenerateFixtures();

        // Klasse 4 ist frei
        var (response, content) = await _factory.PostHttpContent(
            "/api/languageweeks",
            new CreateLanguageWeekCommand(4, 1, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 8), 4, 950.00m));

        Assert.Equal(201, (int)response.StatusCode);
        var lwFromDb = _factory.QueryDatabase(db => db.Languageweeks.First(l => l.Schoolclass.Id == 4));
        Assert.True(lwFromDb.Id != default);
    }

    /// <summary>
    /// Prüft die Validierung und die Businesslogik von POST /api/languageweeks.
    /// </summary>
    [Theory]
    [InlineData(1, 1, "2026-07-01", "2026-07-08", 4, 800.0, 400, "bereits eine Sprachwoche geplant")] // Überschneidung Klasse 1
    [InlineData(4, 1, "2026-05-02", "2026-05-06", 1, 800.0, 400, "Lehrer ist in diesem Zeitraum bereits auf einer anderen Sprachwoche")] // Überschneidung Lehrer 1
    [InlineData(4, 1, "2021-07-01", "2021-07-08", 4, 800.0, 400, "Start der Sprachwoche muss in der Zukunft liegen")] // Model Validation
    [InlineData(4, 1, "2027-07-08", "2027-07-01", 4, 800.0, 400, "Enddatum muss nach dem Startdatum liegen")] // Model Validation
    [InlineData(999, 1, "2026-07-01", "2026-07-08", 4, 800.0, 400, "Klasse nicht gefunden")] // FK not found
    [InlineData(4, 999, "2026-07-01", "2026-07-08", 4, 800.0, 400, "Reiseziel nicht gefunden")] // FK not found
    [InlineData(4, 1, "2026-07-01", "2026-07-08", 999, 800.0, 400, "Lehrer nicht gefunden")] // FK not found
    public async Task T06_CreateLanguageWeekReturnsErrorTest(
        int classId, int destId, string fromStr, string toStr, int teacherId, double price,
        int expectedStatusCode, string expectedErrorMessage)
    {
        GenerateFixtures();

        var cmd = new CreateLanguageWeekCommand(classId, destId, DateOnly.Parse(fromStr), DateOnly.Parse(toStr), teacherId, (decimal)price);
        var (response, content) = await _factory.PostHttpContent("/api/languageweeks", cmd);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
        Assert.True(_factory.ContentContainsErrorMessage(content, expectedErrorMessage));
    }

    /// <summary>
    /// Prüft, ob PUT /api/languageweeks/1 HTTP 204 liefert.
    /// </summary>
    [Fact]
    public async Task T07_UpdateLanguageWeekReturns204Test()
    {
        GenerateFixtures();

        var cmd = new UpdateLanguageWeekCommand(1, 1, new DateOnly(2026, 5, 10), new DateOnly(2026, 5, 17), 4, 850);
        var (response, content) = await _factory.PutHttpContent("/api/languageweeks/1", cmd);

        Assert.Equal(204, (int)response.StatusCode);
        Assert.Equal(850, _factory.QueryDatabase(db => db.Languageweeks.First(l => l.Id == 1).PricePerPerson));
    }

    /// <summary>
    /// Prüft die Validierung und die Businesslogik von PUT /api/languageweeks/2.
    /// </summary>
    [Theory]
    [InlineData(999, "2026-07-01", "2026-07-08", 4, 800.0, 400, "Reiseziel nicht gefunden")] // FK not found
    [InlineData(1, "2026-07-01", "2026-07-08", 999, 800.0, 400, "Lehrer nicht gefunden")] // FK not found
    [InlineData(1, "2026-05-02", "2026-05-06", 1, 800.0, 400, "Lehrer ist in diesem Zeitraum bereits auf einer anderen Sprachwoche")] // Überschneidung Lehrer 1
    [InlineData(1, "2021-07-01", "2021-07-08", 4, 800.0, 400, "Start der Sprachwoche muss in der Zukunft liegen")] // Model Validation
    [InlineData(1, "2027-07-08", "2027-07-01", 4, 800.0, 400, "Enddatum muss nach dem Startdatum liegen")] // Model Validation
    public async Task T08_UpdateLanguageWeekReturnsErrorTest(
        int destId, string fromStr, string toStr, int teacherId, double price,
        int expectedStatusCode, string expectedErrorMessage)
    {
        GenerateFixtures();

        var cmd = new UpdateLanguageWeekCommand(2, destId, DateOnly.Parse(fromStr), DateOnly.Parse(toStr), teacherId, (decimal)price);
        var (response, content) = await _factory.PutHttpContent("/api/languageweeks/2", cmd);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
        Assert.True(_factory.ContentContainsErrorMessage(content, expectedErrorMessage));
    }

    /// <summary>
    /// Prüft, ob PATCH /api/languageweeks/1/price HTTP 204 liefert.
    /// </summary>
    [Fact]
    public async Task T09_UpdateLanguageWeekPriceReturns204Test()
    {
        GenerateFixtures();

        var cmd = new UpdateLanguageWeekPriceCommand(1, 999.99m);
        var (response, content) = await _factory.PatchHttpContent("/api/languageweeks/1/price", cmd);

        Assert.Equal(204, (int)response.StatusCode);
        Assert.Equal(999.99m, _factory.QueryDatabase(db => db.Languageweeks.First(l => l.Id == 1).PricePerPerson));
    }

    /// <summary>
    /// Prüft, ob DELETE /api/languageweeks/1 HTTP 204 liefert.
    /// </summary>
    [Fact]
    public async Task T10_DeleteLanguageWeekReturns204Test()
    {
        GenerateFixtures();

        var (response, content) = await _factory.DeleteHttpContent("/api/languageweeks/1"); // LW 1 ist in Zukunft ohne Registrierungen

        Assert.Equal(204, (int)response.StatusCode);
        Assert.False(_factory.QueryDatabase(db => db.Languageweeks.Any(l => l.Id == 1)));
    }

    /// <summary>
    /// Prüft, ob DELETE /api/languageweeks/2 HTTP 400 mit der Meldung
    /// Vergangene Sprachwochen dürfen nicht gelöscht werden
    /// liefert.
    /// </summary>
    [Fact]
    public async Task T11_DeleteLanguageWeekReturns400IfPastTest()
    {
        GenerateFixtures();

        var (response, content) = await _factory.DeleteHttpContent("/api/languageweeks/2"); // LW 2 ist in der Vergangenheit

        Assert.Equal(400, (int)response.StatusCode);
        Assert.True(_factory.ContentContainsErrorMessage(content, "Vergangene Sprachwochen dürfen nicht gelöscht werden"));
    }

    /// <summary>
    /// Prüft, ob DELETE /api/languageweeks/3 HTTP 400 mit der Meldung
    /// ...da bereits Schüler angemeldet sind
    /// liefert.
    /// </summary>
    [Fact]
    public async Task T12_DeleteLanguageWeekReturns400IfRegistrationsExistTest()
    {
        GenerateFixtures();

        var (response, content) = await _factory.DeleteHttpContent("/api/languageweeks/3"); // LW 3 hat Registrierungen

        Assert.Equal(400, (int)response.StatusCode);
        Assert.True(_factory.ContentContainsErrorMessage(content, "da bereits Schüler angemeldet sind"));
    }

    /// <summary>
    /// Prüft, ob DELETE /api/languageweeks/3?forceDelete=true HTTP 204 liefert.
    /// liefert.
    /// </summary>
    [Fact]
    public async Task T13_DeleteLanguageWeekWithForceDeleteReturns204Test()
    {
        GenerateFixtures();

        var (response, content) = await _factory.DeleteHttpContent("/api/languageweeks/3?forceDelete=true"); // LW 3 mit forceDelete

        Assert.Equal(204, (int)response.StatusCode);
        Assert.False(_factory.QueryDatabase(db => db.Languageweeks.Any(l => l.Id == 3)));
    }

    public void Dispose()
    {
        _factory.Dispose();
        _connection.Dispose();
    }
}
