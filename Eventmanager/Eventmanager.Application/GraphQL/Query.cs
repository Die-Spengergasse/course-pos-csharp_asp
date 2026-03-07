using Eventmanager.Infrastructure;
using Eventmanager.Model;
using HotChocolate.Data;
using System.Linq;

namespace Eventmanager.Application.GraphQL;

/// <summary>
/// Registierung aller Entities für GraphQL.
/// Wird in Program.cs mit builder.Services.AddGraphQLServer()....AddType<Entity>() verwendet.
/// [UseProjection] ensures that GraphQL queries are translated into SQL SELECTs.
/// [UseFiltering] allows the client to filter data (e.g., by name).
/// [UseSorting] allows the client to sort the results.
/// 
/// Requires HotChocolate.Data.EntityFramework
/// </summary>
public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Event> GetEvents(EventContext db)
        => db.Events;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Show> GetShows(EventContext db)
        => db.Shows;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Guest> GetGuests(EventContext db)
        => db.Guests;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Contingent> GetContingents(EventContext db)
        => db.Contingents;
}
