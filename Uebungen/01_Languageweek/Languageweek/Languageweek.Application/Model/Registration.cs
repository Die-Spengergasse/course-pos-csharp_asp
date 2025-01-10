using System;
using System.Diagnostics.CodeAnalysis;

namespace Languageweek.Application.Model
{
    public class Registration : Entity
    {
        private Registration() { }
        [SetsRequiredMembers]
        public Registration(LanguageWeek languageweek, Student student, DateTime registerDate)
        {
            Languageweek = languageweek;
            Student = student;
            RegisterDate = registerDate;
        }
        public required LanguageWeek Languageweek { get; set; }
        public required Student Student { get; set; }
        public required DateTime RegisterDate { get; set; }
    }
}