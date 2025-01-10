using System;

namespace AspShowcase.Models
{
    // CREATE TABLE Person (
    //    Id          INTEGER PRIMARY KEY AUTO_INCREMENT,
    //    Firstname   VARCHAR(255) NOT NULL,
    //    Lastname    VARCHAR(255) NOT NULL,
    //    DateOfBirth DATETIME
    // );
    public class Person
    {
        #pragma warning disable CS8618 // For EF Core
        protected Person() { }
        #pragma warning restore CS8618
        public Person(string firstname, string lastname, DateTime? dateOfBirth = null)
        {
            Firstname = firstname;
            Lastname = lastname;
            DateOfBirth = dateOfBirth;
        }

        public int Id { get; private set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
