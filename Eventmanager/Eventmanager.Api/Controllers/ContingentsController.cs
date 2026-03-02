using Eventmanager.Application.Commands;
using Eventmanager.Application.Dtos;
using Eventmanager.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContingentsController : ControllerBase
{
    private readonly IEventService _eventService;

    public ContingentsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ContingentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContingentDto>> GetContingentById(int id)
    {
        var contingent = await _eventService.Contingents
            .Where(c => c.Id == id)
            .Select(c => new ContingentDto(
                c.Id, c.Show.Id, c.Show.Date,
                c.Show.Event.Name, c.ContingentType.ToString(), c.AvailableTickets))
            .FirstOrDefaultAsync();
        if (contingent is null) return Problem("Contingent not found.", statusCode: 404);
        return Ok(contingent);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContingent([FromBody] CreateContingentCmd cmd)
    {
        try
        {
            var contingent = await _eventService.CreateContingent(cmd);
            // Return 201 created and set the location header to the uri of the created resource (/api/contingents/{id})
            return CreatedAtAction(
                nameof(GetContingentById),      // Name of GET Method to retrieve the resource.
                new { id = contingent.Id },     // Routing parameter (id)
                new { id = contingent.Id });    // Created resource.
        }
        catch (EventServiceException e)
        {
            return Problem(e.Message, statusCode: 400);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContingent([FromRoute] int id, [FromBody] UpdateContingentCmd cmd)
    {
        if (id != cmd.Id) return Problem("Invalid id.", statusCode: 400);
        try
        {
            await _eventService.UpdateContingent(cmd);
            return NoContent();
        }
        catch (EventServiceNotFoundException e)
        {
            return Problem(e.Message, statusCode: 404);
        }
        catch (EventServiceException e)
        {
            return Problem(e.Message, statusCode: 400);
        }
    }

    [HttpPatch("{id}/availableTickets")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailableTickets([FromRoute] int id, [FromBody] UpdateContingentAvailableTicketsCmd cmd)
    {
        if (id != cmd.Id) return Problem("Invalid id.", statusCode: 400);
        try
        {
            await _eventService.UpdateAvailableTickets(cmd);
            return NoContent();
        }
        catch (EventServiceNotFoundException e)
        {
            return Problem(e.Message, statusCode: 404);
        }
        catch (EventServiceException e)
        {
            return Problem(e.Message, statusCode: 400);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContingents([FromRoute] int id)
    {
        try
        {
            await _eventService.DeleteContingent(id);
            return NoContent();
        }
        catch (EventServiceNotFoundException e)
        {
            return Problem(e.Message, statusCode: 404);
        }
        catch (EventServiceException e)
        {
            return Problem(e.Message, statusCode: 400);
        }
    }
}
