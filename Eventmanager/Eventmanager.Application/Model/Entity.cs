
using Eventmanager.Application.GraphQL;
namespace Eventmanager.Application.Model;

public abstract class Entity
{
    [UseHashedId]
    public int Id { get; set; }
}
