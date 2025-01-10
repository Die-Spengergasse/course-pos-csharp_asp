using Eventmanager.Application.Dtos;
using Eventmanager.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly EventContext _db;

    public EventsController(EventContext db)
    {
        _db = db;
    }
    [HttpGet]
    [ProducesResponseType<List<EventDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventDto>>> GetAllEvents()
    {
        var events = await _db.Events
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
        var eventData = await _db.Events
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

