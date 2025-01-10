using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Languageweek.Application.Model
{
    public class Schoolclass : Entity
    {
        private Schoolclass() { }
        [SetsRequiredMembers]
        public Schoolclass(string shortname, string department)
        {
            Shortname = shortname;
            Department = department;
        }
        public required string Shortname { get; set; }
        public required string Department { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<LanguageWeek> Languageweeks { get; } = new();
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Student> Students { get; } = new();
    }
}