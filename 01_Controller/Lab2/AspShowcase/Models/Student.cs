using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspShowcase.Models
{
    public class Student
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected Student()
        { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Student(int studentNr, string firstname, string lastname, string email)
        {
            StudentNr = studentNr;
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]  // Damit EF Core kein AUTO_INCREMENT Feld anlegt.
        public int StudentNr { get; set; }     // PK, von extern

        public Guid Guid { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public List<Handin> Handins { get; } = new();
    }
}