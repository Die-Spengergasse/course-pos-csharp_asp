using Languageweek.Application.Commands;
using Languageweek.Application.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Languageweek.Application.Services
{
    public interface IRegistrationService
    {
        IQueryable<Registration> Registrations { get; }

        Task<Registration> CreateRegistration(CreateRegistrationCmd cmd);
        Task<Registration> DeleteRegistration(int id);
    }
}