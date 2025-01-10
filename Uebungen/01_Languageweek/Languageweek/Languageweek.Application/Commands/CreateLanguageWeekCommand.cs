using System;

namespace Languageweek.Application.Commands;

// TODO: Add validations
public record CreateLanguageWeekCommand(
    int SchoolclassId,
    int DestinationId,
    DateOnly From,
    DateOnly To,
    int TeacherId,
    decimal PricePerPerson
);