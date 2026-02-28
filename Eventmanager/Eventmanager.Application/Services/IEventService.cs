using Eventmanager.Application.Commands;
using Eventmanager.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Application.Services;

public interface IEventService
{
    IQueryable<Contingent> Contingents { get; }
    Task<Contingent> CreateContingent(CreateContingentCmd cmd);
    Task UpdateContingent(UpdateContingentCmd cmd);
    Task UpdateAvailableTickets(UpdateContingentAvailableTicketsCmd cmd);
    Task DeleteContingent(int id);
}