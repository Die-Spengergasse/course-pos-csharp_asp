using Eventmanager.Api.Controllers;
using Eventmanager.Application.Commands;
using Eventmanager.Application.Dtos;
using Eventmanager.Application.Services;
using Eventmanager.Model;
using Microsoft.AspNetCore.Mvc;
using MockQueryable;    // NuGet: MockQueryable.NSubstitute
using NSubstitute;      // NuGet: NSubstitute
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eventmanager.Test;

/// <summary>
/// True unit tests to demonstrate mocking with NSubstitute.
/// The controller is tested in isolation, without a real database in the background.
/// </summary>
public class ContingentsControllerWithServiceMockTests
{
    private readonly IEventService _eventServiceMock;
    private readonly ContingentsController _controller;
    private readonly DateTime _now = DateTime.Now;

    public ContingentsControllerWithServiceMockTests()
    {
        // 1. Arrange: We create a mock object (substitute) for the interface
        _eventServiceMock = Substitute.For<IEventService>();

        // The controller receives the mock object instead of the real service (Dependency Injection)
        _controller = new ContingentsController(_eventServiceMock);
    }

    /// <summary>
    /// Tests the success case when creating a contingent.
    /// Shows how to configure return values for method calls (.Returns).
    /// </summary>
    [Fact]
    public async Task CreateContingent_ReturnsCreated_WhenServiceSucceeds()
    {
        // Arrange
        var cmd = new CreateContingentCmd(1, "Floor", 100);

        // We create dummy data that the mocked service should return
        var dummyEvent = new Event("Test Event") { Id = 1 };
        var dummyShow = new Show(dummyEvent, _now) { Id = 1 };
        var expectedContingent = new Contingent(dummyShow, ContingentType.Floor, 100) { Id = 42 };

        // MOCK CONFIGURATION: 
        // When CreateContingent is called with our command, return 'expectedContingent'.
        _eventServiceMock.CreateContingent(cmd).Returns(expectedContingent);

        // Act
        var result = await _controller.CreateContingent(cmd);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetContingentById", createdAtActionResult.ActionName);

        // VERIFICATION: 
        // We ensure that the controller actually called the method in the service (exactly once).
        await _eventServiceMock.Received(1).CreateContingent(cmd);
    }

    /// <summary>
    /// Tests error handling during deletion.
    /// Shows how to simulate exceptions without having to set up complex database states.
    /// </summary>
    [Fact]
    public async Task DeleteContingent_Returns404_WhenServiceThrowsNotFoundException()
    {
        // Arrange
        int invalidId = 999;

        // MOCK CONFIGURATION:
        // We force the mock to throw an exception when trying to delete ID 999.
        // This is much easier than setting up a real empty database!
        _eventServiceMock
            .When(x => x.DeleteContingent(invalidId))
            .Throw(new EventServiceNotFoundException("Contingent not found."));

        // Act
        var result = await _controller.DeleteContingents(invalidId);

        // Assert
        // We expect the controller to catch the exception and convert it into a ProblemDetails with status 404.
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(404, problemDetails.Status);
    }

    /// <summary>
    /// Tests the GET endpoint. 
    /// Demonstrates the complex mocking of IQueryable for asynchronous EF Core commands 
    /// (like FirstOrDefaultAsync) using MockQueryable.NSubstitute.
    /// </summary>
    [Fact]
    public async Task GetContingentById_Returns200AndDto_WhenContingentExists()
    {
        // 1. Arrange: Setup dummy data
        var dummyEvent = new Event("Rock am Ring") { Id = 10 };
        var dummyShow = new Show(dummyEvent, _now) { Id = 20 };

        // We create a normal list of objects in memory
        var contingentsList = new List<Contingent>
        {
            new Contingent(dummyShow, ContingentType.Floor, 100) { Id = 1 },
            new Contingent(dummyShow, ContingentType.Rang, 50) { Id = 2 }
        };

        // MOCK CONFIGURATION FOR IQUERYABLE:
        // BuildMock() is the extension method from MockQueryable. 
        // It wraps our list so that asynchronous EF Core calls (FirstOrDefaultAsync) 
        // work with it without needing a database.
        var mockQueryable = contingentsList.BuildMock();
        _eventServiceMock.Contingents.Returns(mockQueryable);

        // 2. Act: Call the controller
        // We search for ID 2, which exists in our mocked list.
        var actionResult = await _controller.GetContingentById(2);

        // 3. Assert: Verify results
        // Since the method returns ActionResult<ContingentDto>, we check the 'Result'
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        // Extract the actual value (the DTO) from the OkObjectResult
        var returnedDto = Assert.IsType<ContingentDto>(okResult.Value);

        // Do the mapped values match?
        Assert.Equal(2, returnedDto.Id);
        Assert.Equal("Rang", returnedDto.ContingentType);
        Assert.Equal("Rock am Ring", returnedDto.EventName);
        Assert.Equal(50, returnedDto.AvailableTickets);
    }
}
