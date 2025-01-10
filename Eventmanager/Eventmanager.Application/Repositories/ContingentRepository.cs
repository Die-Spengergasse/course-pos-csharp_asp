using Eventmanager.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Eventmanager.Application.Repositories;

public class ContingentRepository(DbContext db, TimeProvider timeProvider) : Repository<Contingent>(db), IContingentRepository
{
    public async Task Update(Contingent entity, long version)
    {
        if (entity.Version != version)
            throw new DbUpdateConcurrencyException($"The contingent has already changed.");

        entity.Version = timeProvider.GetTimestamp();
        await base.UpdateAndSave(entity);
    }

    public override Task UpdateAndSave(Contingent entity) => Update(entity, default);
}
