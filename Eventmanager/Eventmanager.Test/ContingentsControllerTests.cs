using Eventmanager.Application.Commands;
using Eventmanager.Application.Dtos;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class ContingentsControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory<EventContext> _factory;
    private readonly TimeProvider _timeProvider =
        new FakeTimeProvider(new DateTimeOffset(2026, 2, 28, 16, 30, 0, TimeSpan.Zero));
    private readonly SqliteConnection _connection =
        new SqliteConnection("DataSource=:memory:");
    public ContingentsControllerTests()
    {
        _factory = new TestWebApplicationFactory<EventContext>();
        _factory.SubstituteService<EventContext>(opt => opt.UseSqlite(_connection));
        _factory.SubstituteService<TimeProvider>(opt => _timeProvider, ServiceLifetime.Singleton);
    }
    /// <summary>
    /// Creates global test data for all unit tests.
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
    /// Checks whether GET /api/contingents/{id} returns HTTP 200 and a ContingentDto.
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
    /// Checks whether POST /api/contingents returns HTTP 201 with ID and modifies the database.
    /// </summary>
    [Fact]
    public async Task CreateContingentReturns201Test()
    {
        GenerateFixtures();
        var (response, content) = await _factory.PostHttpContent(
            "/api/contingents",
            new CreateContingentCmd(1, "Floor", 10));
        Assert.Equal(201, (int)response.StatusCode);
        Assert.True(_factory.QueryDatabase(db => db.Contingents.Any(c => c.Id == 2)));
        Assert.True(content.GetProperty("id").GetInt32() != default);
    }
    /// <summary>
    /// Checks whether PUT /api/contingents/{id} returns HTTP 204 and modifies the database.
    /// </summary>
    [Fact]
    public async Task UpdateContingentReturns204Test()
    {
        GenerateFixtures();
        var (response, content) = await _factory.PutHttpContent(
            "/api/contingents/1",
            new UpdateContingentCmd(1, 1, "Floor", 10, 0));
        Assert.Equal(204, (int)response.StatusCode);
        Assert.True(_factory.QueryDatabase(
            db => db.Contingents.Any(
                c => c.Id == 1 && c.ContingentType == ContingentType.Floor)));
    }
    /// <summary>
    /// Checks whether DELETE /api/contingents/{id} returns HTTP 204 and modifies the database.
    /// </summary>
    [Fact]
    public async Task DeleteContingentReturns204Test()
    {
        GenerateFixtures();
        var (response, content) = await _factory.DeleteHttpContent("/api/contingents/1");
        Assert.Equal(204, (int)response.StatusCode);
        Assert.False(_factory.QueryDatabase(db => db.Contingents.Any(c => c.Id == 1)));
    }
    /// <summary>
    /// Checks whether POST /api/contingents returns HTTP 400 with the corresponding error message in the problem detail.
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
        _connection.Dispose();
    }
}
