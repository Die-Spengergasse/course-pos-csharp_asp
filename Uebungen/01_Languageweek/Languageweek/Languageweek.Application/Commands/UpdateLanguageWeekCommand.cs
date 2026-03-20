using System;

namespace Languageweek.Application.Commands;

// TODO: Add validations
public record UpdateLanguageWeekCommand(
    int Id,
    int DestinationId,
    DateOnly From,
    DateOnly To,
    int TeacherId,
    decimal PricePerPerson,
    int? SupportTeacherId
);
