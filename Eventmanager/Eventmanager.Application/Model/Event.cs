using Eventmanager.Application.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eventmanager.Model
{
    public class Event : Entity
    {
        protected Event() { }
        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Event(string name)
        {
            Name = name;
        }
        public required string Name { get; set; }
        [JsonIgnore]
        // Setter is important for UseProjection in HotChocolate's GraphQL
        public List<Show> Shows { get; set; } = new();
    }
}