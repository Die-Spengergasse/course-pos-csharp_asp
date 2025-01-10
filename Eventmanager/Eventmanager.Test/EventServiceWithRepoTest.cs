using Eventmanager.Application.Commands;
using Eventmanager.Application.Repositories;
using Eventmanager.Application.Services;
using Eventmanager.Model;
using NSubstitute;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class EventServiceWithRepoTest
{
    private readonly IContingentRepository _contingentRepo;
    private readonly EventServiceWithRepo _service;

    // Ein fixes Datum für reproduzierbare Tests statt DateTime.Now
    private readonly DateTime _defaultDate = new DateTime(2026, 3, 9, 14, 0, 0);

    public EventServiceWithRepoTest()
    {
        _contingentRepo = Substitute.For<IContingentRepository>();
        _service = new EventServiceWithRepo(_contingentRepo, isDevelopment: true);
    }

    private (Event[] events, Show[] shows, Contingent[] contingents) GenerateFixtures()
    {
        var events = new Event[] { new Event("Event1") { Id = 1 } };
        var shows = new Show[] { new Show(events[0], new DateTime(2026, 6, 12, 18, 0, 0)) { Id = 1 } };
        var contingents = new Contingent[] { new Contingent(shows[0], ContingentType.Rang, 100) };
        return (events, shows, contingents);
    }
    [Fact]
    public async Task CreateContingent_WithValidData_CreatesAndSavesContingent()
    {
        // Arrange
        var show = GenerateFixtures().shows[0];
        var cmd = new CreateContingentCmd(1, "Floor", 10);
        _contingentRepo.FindByIdAsync<Show>(1).Returns(show);
        _contingentRepo.ExistsAsync(Arg.Any<Expression<Func<Contingent, bool>>>()).Returns(false);

        // Act
        var result = await _service.CreateContingent(cmd);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ContingentType.Floor, result.ContingentType);
        Assert.Equal(10, result.AvailableTickets);
        await _contingentRepo.Received(1).CreateAndSave(result);
    }

    [Fact]
    public async Task CreateContingent_WithInvalidContingentType_ThrowsException()
    {
        // Arrange
        var cmd = new CreateContingentCmd(1, "InvalidType", 10);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EventServiceException>(() => _service.CreateContingent(cmd));
        Assert.Equal("InvalidType is not a valid contingent type.", ex.Message);
        await _contingentRepo.DidNotReceive().CreateAndSave(Arg.Any<Contingent>());
    }

    [Fact]
    public async Task CreateContingent_WhenShowDoesNotExist_ThrowsException()
    {
        // Arrange
        var cmd = new CreateContingentCmd(999, "Floor", 10);
        _contingentRepo.FindByIdAsync<Show>(999).Returns(null as Show);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EventServiceException>(() => _service.CreateContingent(cmd));
        Assert.Equal("Show 999 not found.", ex.Message);
        await _contingentRepo.DidNotReceive().CreateAndSave(Arg.Any<Contingent>());
    }

    [Fact]
    public async Task CreateContingent_WhenContingentAlreadyExists_ThrowsException()
    {
        // Arrange
        var show = GenerateFixtures().shows[0];
        var cmd = new CreateContingentCmd(1, "Rang", 10);
        _contingentRepo.FindByIdAsync<Show>(1).Returns(show);
        _contingentRepo.ExistsAsync(Arg.Any<Expression<Func<Contingent, bool>>>()).Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EventServiceException>(() => _service.CreateContingent(cmd));
        Assert.Equal("Show has already a contingent for Rang.", ex.Message);
        await _contingentRepo.DidNotReceive().CreateAndSave(Arg.Any<Contingent>());
    }

    [Fact]
    public async Task DeleteContingent_WhenContingentHasNoTickets_DeletesAndSaves()
    {
        // Arrange
        var contingent = GenerateFixtures().contingents[0];
        _contingentRepo.FindByIdAsync(1, nameof(Contingent.Tickets)).Returns(contingent);

        // Act
        await _service.DeleteContingent(1);

        // Assert
        await _contingentRepo.Received(1).DeleteAndSave(contingent);
    }

    [Fact]
    public async Task DeleteContingent_WhenContingentHasTickets_ThrowsException()
    {
        // Arrange
        var contingent = GenerateFixtures().contingents[0];
        contingent.Tickets.Add(
            new Ticket(
                new Guest("first", "last", new DateOnly(2004, 2, 1)),
                contingent,
                TicketState.Sold,
                new DateTime(2026, 3, 19, 14, 0, 0), 2));
        _contingentRepo.FindByIdAsync(1, nameof(Contingent.Tickets)).Returns(contingent);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EventServiceException>(() => _service.DeleteContingent(1));
        Assert.Equal("The contingent has already sold or reserved all the tickets.", ex.Message);
        // Sicherstellen, dass nichts gelöscht wird
        await _contingentRepo.DidNotReceive().DeleteAndSave(Arg.Any<Contingent>());
    }

    [Fact]
    public async Task DeleteContingent_WhenContingentDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _contingentRepo.FindByIdAsync(Arg.Any<int>(), nameof(Contingent.Tickets))
            .Returns(null as Contingent);
        // Act & Assert
        var ex = await Assert.ThrowsAsync<EventServiceNotFoundException>(() => _service.DeleteContingent(1));
        Assert.Equal("Contingent 1 not found.", ex.Message);
        // Sicherstellen, dass nichts gelöscht wird
        await _contingentRepo.DidNotReceive().DeleteAndSave(Arg.Any<Contingent>());
    }
}
