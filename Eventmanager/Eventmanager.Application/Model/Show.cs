using Eventmanager.Application.Model;
using System;
using System.Collections.Generic;

namespace Eventmanager.Model
{
    public class Show : Entity
    {
        protected Show() { }
        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Show(Event @event, DateTime date)
        {
            Event = @event;
            Date = date;
        }
        public required Event Event { get; set; }
        public required DateTime Date { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        // Setter is important for UseProjection in HotChocolate's GraphQL
        public List<Contingent> Contingents { get; set; } = new();
    }
}