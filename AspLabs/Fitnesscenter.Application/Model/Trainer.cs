#pragma warning disable CS8618
using System.Collections.Generic;

namespace Fitnesscenter.Model;

public class Trainer
{
    public Trainer(string firstName, string lastName, string specialization, bool isExternal)
    {
        FirstName = firstName;
        LastName = lastName;
        Specialization = specialization;
        IsExternal = isExternal;
    }

    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
    public bool IsExternal { get; set; }

    public List<TrainingSession> TrainingSessions { get; } = new();
}

#pragma warning restore CS8618
