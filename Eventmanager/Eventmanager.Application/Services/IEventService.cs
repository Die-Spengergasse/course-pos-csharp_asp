using Eventmanager.Application.Commands;
using Eventmanager.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Eventmanager.Services;

public interface IEventService
{
    IQueryable<Contingent> Contingents { get; }

    Task<Contingent> CreateContingent(CreateContingentCmd cmd);
}