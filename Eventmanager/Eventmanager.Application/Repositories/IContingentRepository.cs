using Eventmanager.Model;
using System.Threading.Tasks;

namespace Eventmanager.Application.Repositories;

public interface IContingentRepository : IRepository<Contingent>
{
    Task Update(Contingent entity, long version);
}