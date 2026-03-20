using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Languageweek.Application.Model
{
    public class LanguageWeek : Entity
    {
        private LanguageWeek() { }
        [SetsRequiredMembers]
        public LanguageWeek(
            Schoolclass schoolclass, Destination destination, DateOnly from, DateOnly to,
            Teacher teacher, decimal pricePerPerson, Teacher? supportTeacher = null)
        {
            Schoolclass = schoolclass;
            Destination = destination;
            From = from;
            To = to;
            Teacher = teacher;
            PricePerPerson = pricePerPerson;
            SupportTeacher = supportTeacher;
        }
        public required Schoolclass Schoolclass { get; set; }
        public required Destination Destination { get; set; }
        public required DateOnly From { get; set; }
        public required DateOnly To { get; set; }
        public required Teacher Teacher { get; set; }
        public required decimal PricePerPerson { get; set; }
        public Teacher? SupportTeacher { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Registration> Registrations { get; } = new();
    }
}