using Eventmanager.Application.Commands;
using Eventmanager.Application.Services;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using Microsoft.Extensions.Time.Testing;
using MockQueryable.NSubstitute;
using NSubstitute;
using System;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class EventServiceWithDbMockTests
{
    private readonly TimeProvider _timeProvider;
    private readonly EventContext _db;

    public EventServiceWithDbMockTests()
    {
        _timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 3, 9, 14, 0, 0, TimeSpan.Zero));
        _db = Substitute.For<EventContext>();
    }


    private Tentity[] AddEntities<Tentity>(params Tentity[] items) where Tentity : class
    {
        var mockSet = items.BuildMockDbSet();
        _db.Set<Tentity>().Returns(mockSet);
        return items;
    }

    /// <summary>
    /// Erstellt zentral alle fake DbSets.
    /// </summary>
    private void GenerateFixtures()
    {
        var events = AddEntities(new Event("Event") { Id = 1 });
        var shows = AddEntities(new Show(events[0], new DateTime(2026, 3, 9, 14, 0, 0)) { Id = 1 });
        var contingents = AddEntities(new Contingent(shows[0], ContingentType.Rang, 10) { Id = 1 });
    }

    [Fact]
    public async Task CreateContingentCallsAddAndSaveChangesAsyncTest()
    {
        GenerateFixtures();

        var service = new EventService(_db, _timeProvider, true);
        var contingentCmd = new CreateContingentCmd(1, "Floor", 10);
        await service.CreateContingent(contingentCmd);
        _db.Received().Add(
            Arg.Is<Contingent>(c => c.ContingentType == ContingentType.Floor));
        await _db.Received().SaveChangesAsync();
    }

    [Theory]
    [InlineData(999, "Floor", "Show 999 not found")]
    [InlineData(1, "XXX", "not a valid contingent type")]
    [InlineData(1, "Rang", "Show has already a contingent for")]
    public async Task CreateContingentThrowsEventServiceExceptionTest(
        int showId, string contingentType, string expectedErrorMessage)
    {
        GenerateFixtures();

        var service = new EventService(_db, _timeProvider, true);
        var contingentCmd = new CreateContingentCmd(showId, contingentType, 10);
        var ex = await Assert.ThrowsAsync<EventServiceException>(
            () => service.CreateContingent(contingentCmd));
        Assert.Contains(expectedErrorMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
