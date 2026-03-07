using Eventmanager.Application.Commands;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Application.Services;


public class EventService(EventContext db, TimeProvider timeProvider, bool isDevelopment) : IEventService
{
    public IQueryable<Contingent> Contingents => db.Contingents.AsQueryable();
    public IQueryable<Event> Events => db.Events.AsQueryable();
    public IQueryable<Show> Shows => db.Shows.AsQueryable();

    public async Task<Contingent> CreateContingent(CreateContingentCmd cmd)
    {
        if (!Enum.TryParse<ContingentType>(cmd.ContingentType, ignoreCase: true, out var contingentType))
            throw new EventServiceException($"{cmd.ContingentType} is not a valid contingent type.");

        var show = await db.Shows.FirstOrDefaultAsync(s => s.Id == cmd.ShowId)
            ?? throw new EventServiceException($"Show {cmd.ShowId} not found.");

        if (await db.Contingents.AnyAsync(c => c.Show.Id == cmd.ShowId && c.ContingentType == contingentType))
            throw new EventServiceException($"Show has already a contingent for {cmd.ContingentType}.");

        var contingent = new Contingent(show, contingentType, cmd.AvailableTickets);
        db.Add(contingent);
        await SaveChanges();
        return contingent;
    }

    public async Task UpdateContingent(UpdateContingentCmd cmd)
    {
        if (!Enum.TryParse<ContingentType>(cmd.ContingentType, ignoreCase: true, out var contingentType))
            throw new EventServiceException($"{cmd.ContingentType} is not a valid contingent type.");

        var contingent = await db.Contingents
            .FirstOrDefaultAsync(c => c.Id == cmd.Id)
            ?? throw new EventServiceNotFoundException($"Contingent {cmd.Id} not found.");

        if (contingent.Version != cmd.Version)
            throw new EventServiceException($"The contingent has already changed.");

        var show = await db.Shows
            .FirstOrDefaultAsync(s => s.Id == cmd.ShowId)
            ?? throw new EventServiceException($"Show {cmd.ShowId} not found.");

        contingent.Show = show;
        contingent.ContingentType = contingentType;
        contingent.AvailableTickets = cmd.AvailableTickets;
        contingent.Version = timeProvider.GetTimestamp();
        // UPDATE "Contingents" SET "AvailableTickets" = 80, "ContingentType" = Floor, "Version" = 639080503086892609
        // WHERE "Id" = 1 AND "Version" = 0
        // RETURNING 1;
        await SaveChanges();
    }

    public async Task UpdateAvailableTickets(UpdateContingentAvailableTicketsCmd cmd)
    {
        var contingent = await db.Contingents
            .Include(c => c.Tickets)
            .FirstOrDefaultAsync(c => c.Id == cmd.Id)
            ?? throw new EventServiceNotFoundException($"Contingent {cmd.Id} not found.");

        if (cmd.AvailableTickets < contingent.Tickets.Sum(t => t.Pax + 1))
            throw new EventServiceException($"The number of tickets sold or reserved exceeds the number of tickets currently available.");

        contingent.AvailableTickets = cmd.AvailableTickets;
        contingent.Version = timeProvider.GetTimestamp();
        await SaveChanges();
    }

    public async Task DeleteContingent(int id)
    {
        var contingent = await db.Contingents
            .Include(c => c.Tickets)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new EventServiceNotFoundException($"Contingent {id} not found.");

        if (contingent.Tickets.Any())
            throw new EventServiceException("The contingent has already sold or reserved all the tickets.");

        db.Contingents.Remove(contingent);
        await SaveChanges();
    }

    private async Task SaveChanges()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            var message = isDevelopment ? e.InnerException?.Message ?? e.Message : "The record has already changed.";
            throw new EventServiceException(message);
        }
        catch (DbUpdateException e)
        {
            var message = isDevelopment ? e.InnerException?.Message ?? e.Message : "Cannot write changes to database.";
            throw new EventServiceException(message);
        }
    }
}