using System;
using System.Collections.Generic;

namespace AspShowcase.Models
{
    public class Teacher
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected Teacher()
        { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Teacher(string firstname, string lastname, string email)
        {
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
        }

        // Convention: Id --> Primary key
        // int --> Auto_increment, daher NICHT im Konstruktor!
        public int Id { get; set; }

        public Guid Guid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public List<Task> Tasks { get; } = new();  // Navigation zur n Seite, List
        public string Initials => $"{Firstname[0]}{Lastname[0]}";
    }
}