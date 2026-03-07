using Eventmanager.Application.Commands;
using Eventmanager.Application.Dtos;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class ContingentsControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory<EventContext> _factory;
    private readonly TimeProvider _timeProvider;
    public ContingentsControllerTests()
    {
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 2, 28, 16, 30, 0, TimeSpan.Zero));
        _factory = new TestWebApplicationFactory<EventContext>(_timeProvider);
    }
    /// <summary>
    /// Erstellt zentral Testdaten für alle Unittests.
    /// </summary>
    private void GenerateFixtures()
    {
        _factory.InitializeDatabase(db =>
        {
            var @event = new Event("Event1");
            var show = new Show(@event, new DateTime(2026, 3, 7, 14, 0, 0, DateTimeKind.Utc));
            var contingent = new Contingent(show, ContingentType.Rang, 10);
            db.AddRange(show, contingent);
            db.SaveChanges();
        });
    }
    /// <summary>
    /// Prüft, ob GET /api/contingents/{id} HTTP 200 und ein ContingentDto liefert.
    /// </summary>
    [Fact]
    public async Task GetContingentByIdReturns200WithDataTest()
    {
        GenerateFixtures();
        var (response, contingent) = await _factory.GetHttpContent<ContingentDto>("/api/contingents/1");
        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(contingent);
        Assert.True(contingent.Id != default);
    }
    /// <summary>
    /// Prüft, ob POST /api/contingents HTTP 201 mit ID liefert und die Datenbank ändert.
    /// </summary>
    [Fact]
    public async Task CreateContingentReturns201Test()
    {
        GenerateFixtures();
        var (response, content) = await _factory.PostHttpContent(
            "/api/contingents",
            new CreateContingentCmd(1, "Floor", 10));
        Assert.Equal(201, (int)response.StatusCode);
        var contingentFromDb = _factory.QueryDatabase(db => db.Contingents.First(c => c.Id == 2));
        Assert.True(contingentFromDb.Id != default);
        Assert.True(content.GetProperty("id").GetInt32() != default);
    }
    /// <summary>
    /// Prüft, ob PUT /api/contingents/{id} HTTP 204 liefert und die Datenbank ändert.
    /// </summary>
    [Fact]
    public async Task UpdateContingentReturns204Test()
    {
        GenerateFixtures();
        var (response, content) = await _factory.PutHttpContent(
            "/api/contingents/1",
            new UpdateContingentCmd(1, 1, "Floor", 10, 0));
        Assert.Equal(204, (int)response.StatusCode);
        var contingentFromDb = _factory.QueryDatabase(db => db.Contingents.First(c => c.Id == 1));
        Assert.True(contingentFromDb.ContingentType == ContingentType.Floor);
    }
    /// <summary>
    /// Prüft, ob DELETE /api/contingents/{id} HTTP 204 liefert und die Datenbank ändert.
    /// </summary>
    [Fact]
    public async Task DeleteContingentReturns204Test()
    {
        GenerateFixtures();
        var (response, content) = await _factory.DeleteHttpContent("/api/contingents/1");
        Assert.Equal(204, (int)response.StatusCode);
        var contingentFromDb = _factory.QueryDatabase(db => db.Contingents.FirstOrDefault(c => c.Id == 1));
        Assert.Null(contingentFromDb);
    }
    /// <summary>
    /// Prüft, ob POST /api/contingents HTTP 400 mit der entsprechenden Fehlermeldung im problem detail liefert.
    /// </summary>
    [Theory]
    [InlineData("xxx", 1, "invalid value for contingent type")]
    [InlineData("Floor", 999, "show 999 not found")]
    [InlineData("Rang", 1, "show has already a contingent for")]
    public async Task CreateContingentReturns400WithProblemDetailTest(string contingentType, int showId, string errorMessage)
    {
        GenerateFixtures();
        var (response, content) = await _factory.PostHttpContent(
            "/api/contingents",
            new CreateContingentCmd(showId, contingentType, 10));
        Assert.Equal(400, (int)response.StatusCode);
        Assert.True(_factory.ContentContainsErrorMessage(content, errorMessage));
    }
    public void Dispose()
    {
        _factory.Dispose();
    }
}
