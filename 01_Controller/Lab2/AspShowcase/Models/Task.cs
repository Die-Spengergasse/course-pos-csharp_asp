using System;
using System.Collections.Generic;

namespace AspShowcase.Models
{
    public class Task
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected Task()
        { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Sie können new Task auch ohne maxPoints aufrufen. Der Wert
        // wird dann automatisch auf NULL gesetzt.
        public Task(
            string subject, string title, Team team,
            Teacher teacher, DateTime expirationDate,
            int? maxPoints = null)
        {
            Subject = subject;
            Title = title;
            Team = team;
            Teacher = teacher;
            ExpirationDate = expirationDate;
            MaxPoints = maxPoints;
        }

        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }

        // public int TeamId { get; set; }  // Kein FK Value, da wir ihn nicht brauchen.
        public Team Team { get; set; }   // Navigation

        public Teacher Teacher { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int? MaxPoints { get; set; }
        public List<Handin> Handins { get; } = new();
    }
}