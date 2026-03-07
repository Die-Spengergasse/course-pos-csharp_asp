using Eventmanager.Application.Dtos;
using Eventmanager.Infrastructure;
using Hypermedia;
using IdHasher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eventmanager.Api.Controllers;

/// <summary>
/// Controller to demonstrate the IdEncoder and hypermedia links.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class GuestsController(EventContext db) : ControllerBase   // Primary constructor in C# 12
{
    [HttpGet]
    [ProducesResponseType<List<GuestDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GuestDto>>> GetGuests(CancellationToken cancellationToken)
    {
        // Remember: Add "use Hypermedia". The AddGet, AddDelete, etc. methods are not available without it.
        // First, query the database to retrieve all the necessary information to prevent client site evaluation.
        // Then, add hypermedia links.
        var guests = (
                await db.Guests
                    .Select(g => new { Guest = g, CanDelete = !g.Tickets.Any() })
                    .ToListAsync(cancellationToken)
            ).Select(g => new GuestDto(
                    Id.From(g.Guest.Id), g.Guest.Firstname, g.Guest.Lastname, g.Guest.BirthDate)
                .AddGet(GenerateResourceLink(Id.From(g.Guest.Id)), "details")
                .AddDelete(GenerateResourceLink(Id.From(g.Guest.Id)), "delete", g.CanDelete))
            .ToList();
        return Ok(guests);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<GuestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuestDto>> GetGuestById([FromRoute] Id id)
    {
        var guest = await db.Guests
            .Where(g => g.Id == id.Value)
            .Select(g => new GuestDto(new Id(g.Id), g.Firstname, g.Lastname, g.BirthDate))
            .FirstOrDefaultAsync();

        if (guest is null) return Problem("Guest not found.", statusCode: 404);
        return Ok(guest);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGuest() => throw new NotImplementedException();

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGuest(Id id) => throw new NotImplementedException();

    [HttpDelete("{id}", Name = nameof(DeleteGuest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGuest(Id id) => throw new NotImplementedException();

    private string GenerateResourceLink(Id id) =>
        Url.ActionLink(action: nameof(GetGuestById), values: new { id = id }) ?? string.Empty;
}
