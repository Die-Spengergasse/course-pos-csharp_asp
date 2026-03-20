using System;

namespace Languageweek.Application.Commands;

public record CreateLanguageWeekCommand(
    int SchoolclassId,
    int DestinationId,
    DateOnly From,
    DateOnly To,
    int TeacherId,
    decimal PricePerPerson,
    int? SupportTeacherId
);