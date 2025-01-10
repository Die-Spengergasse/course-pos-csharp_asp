using Eventmanager.Application.Commands;
using Eventmanager.Infrastructure;
using Eventmanager.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Services;


public class EventService : IEventService
{
    private readonly EventContext _db;
    private readonly bool _isDevelopment;

    public EventService(EventContext db, bool isDevelopment)
    {
        _db = db;
        _isDevelopment = isDevelopment;
    }

    public IQueryable<Contingent> Contingents => _db.Contingents.AsQueryable();

    public async Task<Contingent> CreateContingent(CreateContingentCmd cmd)
    {
        var show = await _db.Shows.FirstOrDefaultAsync(s => s.Id == cmd.ShowId);
        if (show is null)
            throw new EventServiceException($"Show {cmd.ShowId} not found.");

        if (!Enum.TryParse<ContingentType>(cmd.ContingentType, ignoreCase: true, out var contingentType))
            throw new EventServiceException($"{cmd.ContingentType} is not a valid contingent type.");

        if (_db.Contingents.Any(c => c.Show.Id == cmd.ShowId && c.ContingentType == contingentType))
            throw new EventServiceException($"Show has already a contingent for {cmd.ContingentType}.");

        var contingent = new Contingent(show, contingentType, cmd.AvailableTickets);
        _db.Add(contingent);
        await SaveChanges();
        return contingent;
    }

    private async Task SaveChanges()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            var message = _isDevelopment ? e.InnerException?.Message ?? e.Message : "Cannot write changes to database.";
            throw new EventServiceException(e.InnerException?.Message ?? e.Message);
        }
    }
}