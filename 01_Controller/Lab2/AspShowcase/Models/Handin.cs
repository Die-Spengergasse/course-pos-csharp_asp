using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspShowcase.Models
{
    public class Handin
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected Handin()
        { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Handin(Student student, Task task, DateTime created, string description, string documentUrl)
        {
            Student = student;
            Task = task;
            Created = created;
            Description = description;
            DocumentUrl = documentUrl;
        }

        public int Id { get; set; }
        public Guid Guid { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [ForeignKey("TaskId")]
        public Task Task { get; set; }

        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
    }
}