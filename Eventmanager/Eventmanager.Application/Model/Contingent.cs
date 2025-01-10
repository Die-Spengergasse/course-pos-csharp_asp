using Eventmanager.Application.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Eventmanager.Model
{
    public class Contingent : Entity
    {
        protected Contingent() { }
        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Contingent(Show show, ContingentType contingentType, int availableTickets)
        {
            Show = show;
            ContingentType = contingentType;
            AvailableTickets = availableTickets;
        }
        public required Show Show { get; set; }
        public required ContingentType ContingentType { get; set; }
        public required int AvailableTickets { get; set; }
        [ConcurrencyCheck]
        public long Version { get; set; }
        // Setter is important for UseProjection in HotChocolate's GraphQL
        public List<Ticket> Tickets { get; set; } = new();
    }
}