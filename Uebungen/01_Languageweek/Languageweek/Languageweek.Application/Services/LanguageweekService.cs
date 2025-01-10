using Languageweek.Application.Commands;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Languageweek.Application.Services;

public class LanguageweekService(LanguageweekContext db, TimeProvider timeProvider) : ILanguageweekService
{
    public async Task<LanguageWeek> CreateLanguageWeekAsync(CreateLanguageWeekCommand command)
    {
        // TODO: Add your implementation
        throw new NotImplementedException();
    }

    public async Task UpdateLanguageWeekAsync(UpdateLanguageWeekCommand command)
    {
        // TODO: Add your implementation
        throw new NotImplementedException();
    }

    public async Task UpdateLanguageWeekPriceAsync(UpdateLanguageWeekPriceCommand command)
    {
        // TODO: Add your implementation
        throw new NotImplementedException();
    }

    public async Task DeleteLanguageWeekAsync(int id, bool forceDelete = false)
    {
        // TODO: Add your implementation
        throw new NotImplementedException();
    }
}