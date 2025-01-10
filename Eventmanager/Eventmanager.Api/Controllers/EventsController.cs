using Eventmanager.Application.Dtos;
using Eventmanager.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Api.Controllers;

/// <summary>
/// Controller for the GET chapter.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class EventsController(EventContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<EventDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventDto>>> GetAllEvents()
    {
        var events = await db.Events
            .Select(e => new EventDto(e.Id, e.Name, e.Shows.Count))
            .ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<EventDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventById(
        [FromRoute] int id,
        [FromQuery] bool includeShows = false)
    {
        var eventData = await db.Events
            .Where(e => e.Id == id)
            .Select(e => new EventWithShowsDto(
                e.Id, e.Name,
                includeShows
                    ? e.Shows.Select(s => new ShowDto(s.Id, s.Date)).ToList()
                    : new List<ShowDto>()))
            .FirstOrDefaultAsync();
        if (eventData is null) return Problem("Event not found", statusCode: 404);
        return Ok(eventData);
    }
}

