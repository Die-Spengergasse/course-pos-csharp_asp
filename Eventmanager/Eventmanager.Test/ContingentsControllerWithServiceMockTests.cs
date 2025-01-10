using Eventmanager.Application.Commands;
using Eventmanager.Application.Dtos;
using Eventmanager.Application.Services;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using MockQueryable;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class ContingentsControllerWithServiceMockTests : IDisposable
{
    private readonly TestWebApplicationFactory<EventContext> _factory;
    private readonly IEventService _eventServiceMock = Substitute.For<IEventService>();
    public ContingentsControllerWithServiceMockTests()
    {
        _factory = new TestWebApplicationFactory<EventContext>();
        _factory.SubstituteService<IEventService>(provider => _eventServiceMock);
    }
    private void GenerateFixtures()
    {
        var events = new List<Event>
        {
            new Event("Event") { Id = 1 }
        };
        var shows = new List<Show>
        {
            new Show(events[0], new DateTime(2026, 3, 9, 14, 0, 0)) { Id = 1 }
        };
        var contingents = new List<Contingent>
        {
            new Contingent(shows[0], ContingentType.Rang, 10) { Id = 1 }
        };
        _eventServiceMock.Contingents.Returns(contingents.BuildMock());
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
        Assert.True(contingent.Id == 1);
    }
    /// <summary>
    /// Checks whether POST /api/contingents returns HTTP 201 with ID and modifies the database.
    /// </summary>
    [Fact]
    public async Task CreateContingentReturns201Test()
    {
        var command = new CreateContingentCmd(1, "Floor", 10);
        _eventServiceMock.CreateContingent(Arg.Any<CreateContingentCmd>()).Returns(opt =>
            new Contingent(
                new Show(new Event("Event"), new DateTime(2026, 3, 9, 20, 0, 0)),
                ContingentType.Floor, 10)
            { Id = 1 });

        var (response, content) = await _factory.PostHttpContent(
            "/api/contingents",
            command);
        await _eventServiceMock.Received().CreateContingent(Arg.Any<CreateContingentCmd>());
        Assert.Equal(201, (int)response.StatusCode);
    }
    /// <summary>
    /// Checks whether POST /api/contingents returns HTTP 400 with the corresponding error message in the problem detail.
    /// </summary>
    [Fact]
    public async Task CreateContingentReturns400IfCreateContingentThrowsEventServiceExceptionTest()
    {
        _eventServiceMock
            .CreateContingent(Arg.Any<CreateContingentCmd>())
            .Throws(info => new EventServiceException("Error message"));
        var (response, content) = await _factory.PostHttpContent(
            "/api/contingents",
            new CreateContingentCmd(1, "Floor", 10));
        await _eventServiceMock.Received().CreateContingent(Arg.Any<CreateContingentCmd>());
        Assert.Equal(400, (int)response.StatusCode);
        Assert.True(_factory.ContentContainsErrorMessage(content, "Error message"));
    }
    public void Dispose()
    {
        _factory.Dispose();
    }
}
