using Eventmanager.Application.Commands;
using Eventmanager.Application.Repositories;
using Eventmanager.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Application.Services;

public class EventServiceWithRepo(IContingentRepository contingentRepo, bool isDevelopment)
{
    public Tresult QueryContingents<Tresult>(Func<IQueryable<Contingent>, Tresult> queryFunc)
        => contingentRepo.Query(queryFunc);

    public async Task<Contingent> CreateContingent(CreateContingentCmd cmd)
    {
        if (!Enum.TryParse<ContingentType>(cmd.ContingentType, ignoreCase: true, out var contingentType))
            throw new EventServiceException($"{cmd.ContingentType} is not a valid contingent type.");

        var show = await contingentRepo.FindByIdAsync<Show>(cmd.ShowId)
            ?? throw new EventServiceException($"Show {cmd.ShowId} not found.");

        if (await contingentRepo.ExistsAsync(c => c.Show.Id == cmd.ShowId && c.ContingentType == contingentType))
            throw new EventServiceException($"Show has already a contingent for {cmd.ContingentType}.");

        var contingent = new Contingent(show, contingentType, cmd.AvailableTickets);
        await RethrowDbException(() => contingentRepo.CreateAndSave(contingent));
        return contingent;
    }

    public async Task UpdateContingent(UpdateContingentCmd cmd)
    {
        if (!Enum.TryParse<ContingentType>(cmd.ContingentType, ignoreCase: true, out var contingentType))
            throw new EventServiceException($"{cmd.ContingentType} is not a valid contingent type.");

        var contingent = await contingentRepo.FindByIdAsync(cmd.Id)
            ?? throw new EventServiceNotFoundException($"Contingent {cmd.Id} not found.");

        var show = await contingentRepo.FindByIdAsync<Show>(cmd.ShowId)
            ?? throw new EventServiceException($"Show {cmd.ShowId} not found.");

        contingent.Show = show;
        contingent.ContingentType = contingentType;
        contingent.AvailableTickets = cmd.AvailableTickets;
        // No version setting!
        await RethrowDbException(() => contingentRepo.Update(contingent, cmd.Version));
    }

    public async Task DeleteContingent(int id)
    {
        var contingent = await contingentRepo.FindByIdAsync(id, nameof(Contingent.Tickets))
            ?? throw new EventServiceNotFoundException($"Contingent {id} not found.");

        if (contingent.Tickets.Any())
            throw new EventServiceException("The contingent has already sold or reserved all the tickets.");

        await RethrowDbException(() => contingentRepo.DeleteAndSave(contingent));
    }

    private async Task RethrowDbException(Func<Task> action)
    {
        try
        {
            await action();
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
