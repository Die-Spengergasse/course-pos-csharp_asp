// =================================================================================================
// Predefined service for the RegistrationController.
// There is nothing to implement or customize here.
// =================================================================================================

using Languageweek.Application.Commands;
using Languageweek.Application.Infrastructure;
using Languageweek.Application.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Languageweek.Application.Services
{
    public class RegistrationService(LanguageweekContext db) : IRegistrationService
    {
        public IQueryable<Registration> Registrations => db.Registrations.AsQueryable();
        public async Task<Registration> CreateRegistration(CreateRegistrationCmd cmd)
        {
            var registration = new Registration(
                await FirstOrThrow<LanguageWeek>(l => l.Id == cmd.LanguageweekId, "Languageweek id not found."),
                await FirstOrThrow<Student>(s => s.Id == cmd.StudentId, "Student id not found."),
                cmd.RegisterDate);
            db.Registrations.Add(registration);
            await SaveOrThrow();
            return registration;
        }
        public async Task<Registration> DeleteRegistration(int id)
        {
            var registration = await FindOrThrow<Registration>(r => r.Id == id, "Registration id not found.");
            db.Registrations.Remove(registration);
            await SaveOrThrow();
            return registration;
        }

        private async Task<TEntity> FirstOrThrow<TEntity>(Expression<Func<TEntity, bool>> predicate, string message) where TEntity : class
            => await db.Set<TEntity>().FirstOrDefaultAsync(predicate) ?? throw new RegistrationServiceException(message);
        private async Task<TEntity> FindOrThrow<TEntity>(Expression<Func<TEntity, bool>> predicate, string message) where TEntity : class
            => await db.Set<TEntity>().FirstOrDefaultAsync(predicate) ?? throw new RegistrationServiceNotFoundException(message);

        private async Task SaveOrThrow()
        {
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new RegistrationServiceException(e.InnerException?.Message ?? e.Message);
            }
        }
    }
}
