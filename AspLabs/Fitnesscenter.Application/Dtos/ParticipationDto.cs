using System;

namespace Fitnesscenter.Application.Dtos;

public record ParticipationDto(
    int Id,
    int TrainingSessionId, DateTime TrainingSessionTime,
    int TrainingSessionRoomId, string TrainingSessionRoomName,
    int TrainingSessionTrainerId, string TrainingSessionTrainerFirstName, string TrainingSessionTrainerLastName);
