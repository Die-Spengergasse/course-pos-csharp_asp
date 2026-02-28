using Fitnesscenter.Application.Commands;
using Fitnesscenter.Infrastructure;
using Fitnesscenter.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitnesscenter.Application.Services;

public class FitnessService
{
    private readonly FitnessContext _db;
    private readonly bool _isDevelopment;
    private readonly TimeProvider _timeProvider;

    public FitnessService(FitnessContext db, bool isDevelopment, TimeProvider timeProvider)
    {
        _db = db;
        _isDevelopment = isDevelopment;
        _timeProvider = timeProvider;
    }

    public async Task<Member> CreateMember(CreateMemberCmd cmd)
    {
        // TODO: Add your implementation
        throw new NotImplementedException();
    }
    public async Task<List<Participation>> CreateParticipations(CreateParticipationCmd cmd)
    {
        // TODO: Add your implementation
        throw new NotImplementedException();
    }

    private async Task SaveChanges()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            var message = _isDevelopment
                ? e.InnerException?.Message ?? e.Message
                : "Cannot write changes to database.";
            throw new FitnessServiceException(e.InnerException?.Message ?? e.Message);
        }
    }
}
