using Microsoft.EntityFrameworkCore;
using System;

namespace AspShowcase.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Team
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected Team()
        { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Team(string name, string schoolclass)
        {
            Name = name;
            Schoolclass = schoolclass;
        }

        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Schoolclass { get; set; }
    }
}