using Eventmanager.Application.Model;
using System;
using System.Collections.Generic;

namespace Eventmanager.Model
{
    public class Guest : Entity
    {
        protected Guest() { }
        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Guest(string firstname, string lastname, DateOnly birthDate)
        {
            Firstname = firstname;
            Lastname = lastname;
            BirthDate = birthDate;
        }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required DateOnly BirthDate { get; set; }
        // Setter is important for UseProjection in HotChocolate's GraphQL
        public List<Ticket> Tickets { get; set; } = new();
    }
}