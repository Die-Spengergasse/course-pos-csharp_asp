using System.Collections.Generic;

namespace Fitnesscenter.Application.Commands;

public record CreateParticipationCmd(int MemberId, List<int> TrainingSessionIds);