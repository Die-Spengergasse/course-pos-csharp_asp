using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Languageweek.Application.Model
{
    public class Student : Entity
    {
        private Student() { }
        [SetsRequiredMembers]
        public Student(string firstname, string lastname, string email, Schoolclass schoolclass, Gender gender, DateOnly dateOfBirth)
        {
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            Schoolclass = schoolclass;
        }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Email { get; set; }
        public required Schoolclass Schoolclass { get; set; }
        public required Gender Gender { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public List<Registration> Registrations { get; } = new();
    }
}