using Eventmanager.Application.Commands;
using Eventmanager.Application.Services;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class EventServiceWithInMemoryDbTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly EventContext _db;
    private readonly FakeTimeProvider _timeProvider;
    private readonly EventService _service;

    public EventServiceWithInMemoryDbTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");

        _db = new EventContext(new DbContextOptionsBuilder().UseSqlite(_conn).Options);
        _db.Database.EnsureDeleted();
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _timeProvider = new FakeTimeProvider(new DateTimeOffset(new DateTime(2026, 3, 9, 16, 0, 0)));

        _service = new EventService(db: _db, timeProvider: _timeProvider, isDevelopment: true);
    }
    /// <summary>
    /// Hilfsmethode, die die übergebenen Parameter in die Datenbank schreibt und diese als
    /// Array zurückgibt.
    /// </summary>
    private Tentity[] AddEntities<Tentity>(params Tentity[] items) where Tentity : class
    {
        _db.Set<Tentity>().AddRange(items);
        _db.SaveChanges();
        return items;
    }
    /// <summary>
    /// Definiert zentrale Musterdaten, mit denen jeder Unittest arbeitet.
    /// </summary>
    private void GenerateFixtures()
    {
        var events = AddEntities(new Event("Event1") { Id = 1 });
        var shows = AddEntities(
            new Show(events[0], new DateTime(2026, 3, 9, 14, 0, 0)) { Id = 1 },
            new Show(events[0], new DateTime(2026, 3, 9, 16, 0, 0)) { Id = 2 });
        var contingents = AddEntities(
            new Contingent(shows[0], ContingentType.Rang, 10) { Id = 1 });
    }
    /// <summary>
    /// Testet den Erfolgsfall der Methode CreateContingent()
    /// </summary>
    [Fact]
    public async Task CreateContingentCreatesNewContingentTest()
    {
        GenerateFixtures();
        await _service.CreateContingent(new CreateContingentCmd(1, "Floor", 10));
        _db.ChangeTracker.Clear();
        Assert.True(_db.Contingents.Any(c => c.ContingentType == ContingentType.Floor && c.AvailableTickets == 10));
    }
    /// <summary>
    /// Testet die geworfene Exception von CreateContingent
    /// basierend auf den Daten des Command Objects.
    /// </summary>
    [Theory]
    [InlineData(1, "x", "x is not a valid contingent type.")]
    [InlineData(999, "Floor", "Show 999 not found.")]
    [InlineData(1, "Rang", "Show has already a contingent for Rang")]
    public async Task CreateContingentThrowsExceptionWithMessageTest(
        int showId, string contingentType, string expectedMessage)
    {
        GenerateFixtures();
        var ex = await Assert.ThrowsAsync<EventServiceException>(() => _service.CreateContingent(
            new CreateContingentCmd(showId, contingentType, 10)));
        Assert.Contains(expectedMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Testet den Erfolgsfall der Methode UpdateContingent()
    /// </summary>
    [Fact]
    public async Task UpdateContingentUpdatesContingentTest()
    {
        GenerateFixtures();
        var expectedVersion = _timeProvider.GetTimestamp();
        await _service.UpdateContingent(new UpdateContingentCmd(1, 1, "Rang", 20, 0));
        _db.ChangeTracker.Clear();
        Assert.True(_db.Contingents.Any(c =>
            c.Id == 1 && c.Show.Id == 1 && c.ContingentType == ContingentType.Rang
            && c.AvailableTickets == 20 && c.Version == expectedVersion));
    }
    /// <summary>
    /// Testet die geworfene Exception von UpdateContingent
    /// basierend auf den Daten des Command Objects.
    /// </summary>
    [Theory]
    [InlineData(999, 1, 0, "Rang", "EventServiceNotFoundException", "Contingent 999 not found.")]
    [InlineData(1, 1, 0, "x", "EventServiceException", "x is not a valid contingent type.")]
    [InlineData(1, 1, 999, "Rang", "EventServiceException", "The contingent has already changed.")]
    [InlineData(1, 999, 0, "Rang", "EventServiceException", "Show 999 not found.")]
    public async Task UpdateContingentThrowsExceptionWithMessageTest(
        int contingentId, int showId, long version, string contingentType,
        string errorType, string expectedMessage)
    {
        GenerateFixtures();
        var ex = await Assert.ThrowsAnyAsync<EventServiceException>(() => _service.UpdateContingent(
            new UpdateContingentCmd(contingentId, showId, contingentType, 20, version)));
        Assert.Equal(errorType, ex.GetType().Name);
        Assert.Contains(expectedMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
