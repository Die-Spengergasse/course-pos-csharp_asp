#pragma warning disable CS8618
using System.Collections.Generic;

namespace Fitnesscenter.Model;

public class Room
{
    public Room(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // z. B. "Cardio", "Kraft", "Yoga"

    public List<TrainingSession> TrainingSessions { get; } = new();
}

#pragma warning restore CS8618
