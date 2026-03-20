using Languageweek.Application.Commands;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Languageweek.Application.Services;

public class LanguageweekService(LanguageweekContext db, TimeProvider timeProvider)
{
    public List<Teacher> GetTeachersWithMinCountOfParticipations(int count)
    {
        throw new NotImplementedException();
    }

    public List<Schoolclass> GetClassesWithoutLanguageWeek()
    {
        throw new NotImplementedException();
    }
    public record SchoolclassStatistics(int Id, string Shortname, int MaleCount, int FemaleCount);
    public List<SchoolclassStatistics> CalcSchoolclassStatistics()
    {
        throw new NotImplementedException();
    }

    public record LanguageWeekRegistrationRate(
        int Id, DateOnly From, DateOnly To, decimal TotalPrice,
        int DestinationId, string DestinationCity, string DestinationCountry,
        int SchoolclassId, string SchoolclassShortname, decimal Percentage);
    public List<LanguageWeekRegistrationRate> CalcRegistrationRates()
    {
        throw new NotImplementedException();
    }

    public async Task<LanguageWeek> CreateLanguageWeekAsync(CreateLanguageWeekCommand cmd)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateLanguageWeekAsync(UpdateLanguageWeekCommand cmd)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateLanguageWeekPriceAsync(UpdateLanguageWeekPriceCommand command)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteLanguageWeekAsync(int id, bool forceDelete = false)
    {
        throw new NotImplementedException();
    }
    private async Task SaveChanges()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new LanguageweekServiceException(e.InnerException?.Message ?? e.Message);
        }
    }
}
