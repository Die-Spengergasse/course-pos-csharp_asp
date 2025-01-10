using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Languageweek.Application.Model
{
    public class Destination : Entity
    {
        private Destination() { }
        [SetsRequiredMembers]
        public Destination(string city, string country)
        {
            City = city;
            Country = country;
        }
        public required string City { get; set; }
        public required string Country { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<LanguageWeek> Languageweeks { get; } = new();
    }
}