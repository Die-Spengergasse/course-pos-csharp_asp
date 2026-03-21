using Eventmanager.Application.Commands;
using Eventmanager.Application.Dtos;
using Eventmanager.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Eventmanager.Api.Endpoints;

public static class ContingentEndpoints
{
    // Diese Extension Method wird später in der Program.cs aufgerufen
    public static RouteGroupBuilder MapContingentEndpoints(this IEndpointRouteBuilder routes)
    {
        // Wir definieren eine Gruppe für alle /minimalapi/contingents Routen
        var group = routes.MapGroup("/minimalapi/contingents");

        // GET /minimalapi/contingents/{id}
        group.MapGet("/{id}", async (int id, IEventService eventService) =>
        {
            var contingent = await eventService.Contingents
                .Where(c => c.Id == id)
                .Select(c => new ContingentDto(
                    c.Id, c.Show.Id, c.Show.Date,
                    c.Show.Event.Name, c.ContingentType.ToString(), c.AvailableTickets))
                .FirstOrDefaultAsync();

            if (contingent is null)
            {
                return Results.Problem("Contingent not found.", statusCode: 404);
            }

            return Results.Ok(contingent);
        })
        .WithName("GetContingentById")
        .Produces<ContingentDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // POST /minimalapi/contingents
        group.MapPost("/", async (CreateContingentCmd cmd, IEventService eventService) =>
        {
            try
            {
                var contingent = await eventService.CreateContingent(cmd);

                // Nutzt den Namen der Route ("GetContingentById"), um den Location-Header korrekt aufzubauen
                return Results.CreatedAtRoute("GetContingentById", new { id = contingent.Id }, new { id = contingent.Id });
            }
            catch (EventServiceException e)
            {
                return Results.Problem(e.Message, statusCode: 400);
            }
        })
        .Produces(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        // PUT /minimalapi/contingents/{id}
        group.MapPut("/{id}", async (int id, UpdateContingentCmd cmd, IEventService eventService) =>
        {
            if (id != cmd.Id) return Results.Problem("Invalid id.", statusCode: 400);

            try
            {
                await eventService.UpdateContingent(cmd);
                return Results.NoContent();
            }
            catch (EventServiceNotFoundException e)
            {
                return Results.Problem(e.Message, statusCode: 404);
            }
            catch (EventServiceException e)
            {
                return Results.Problem(e.Message, statusCode: 400);
            }
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // PATCH /minimalapi/contingents/{id}/availableTickets
        group.MapPatch("/{id}/availableTickets", async (int id, UpdateContingentAvailableTicketsCmd cmd, IEventService eventService) =>
        {
            if (id != cmd.Id) return Results.Problem("Invalid id.", statusCode: 400);

            try
            {
                await eventService.UpdateAvailableTickets(cmd);
                return Results.NoContent();
            }
            catch (EventServiceNotFoundException e)
            {
                return Results.Problem(e.Message, statusCode: 404);
            }
            catch (EventServiceException e)
            {
                return Results.Problem(e.Message, statusCode: 400);
            }
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // DELETE /minimalapi/contingents/{id}
        group.MapDelete("/{id}", async (int id, IEventService eventService) =>
        {
            try
            {
                await eventService.DeleteContingent(id);
                return Results.NoContent();
            }
            catch (EventServiceNotFoundException e)
            {
                return Results.Problem(e.Message, statusCode: 404);
            }
            catch (EventServiceException e)
            {
                return Results.Problem(e.Message, statusCode: 400);
            }
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return group;
    }
}
