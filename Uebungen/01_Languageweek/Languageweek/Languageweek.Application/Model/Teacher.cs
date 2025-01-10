using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Languageweek.Application.Model
{
    public class Teacher : Entity
    {
        private Teacher() { }
        [SetsRequiredMembers]
        public Teacher(string shortname, string firstname, string lastname, string email, Gender gender)
        {
            Shortname = shortname;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Gender = gender;
        }
        public required string Shortname { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Email { get; set; }
        public required Gender Gender { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<LanguageWeek> LanguageweekTeachers { get; } = new();
        [System.Text.Json.Serialization.JsonIgnore]
        public List<LanguageWeek> LanguageweekSupportTeachers { get; } = new();

    }
}