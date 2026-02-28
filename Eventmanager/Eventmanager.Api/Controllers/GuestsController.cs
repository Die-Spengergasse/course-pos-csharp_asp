using Eventmanager.Application.Dtos;
using Eventmanager.Application.Services;
using Eventmanager.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestsController : ControllerBase
    {
        private readonly EventContext _db;

        public GuestsController(EventContext db)
        {
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType<List<GuestDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<EventDto>> GetGuests()
        {
            var guests = await _db.Guests
                .Select(g => new GuestDto(
                    new Id(g.Id), g.Firstname, g.Lastname, g.BirthDate))
                .ToListAsync();
            return Ok(guests);
        }

        [HttpGet("{id}")]
        [ProducesResponseType<GuestDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EventDto>> GetGuestById([FromRoute] Id id)
        {
            var guest = await _db.Guests
                .Where(g => g.Id == id.Value)
                .Select(g => new GuestDto(
                    new Id(g.Id), g.Firstname, g.Lastname, g.BirthDate))
                .FirstOrDefaultAsync();
            if (guest is null) return Problem("Guest not found", statusCode: 404);
            return Ok(guest);
        }
    }
}
