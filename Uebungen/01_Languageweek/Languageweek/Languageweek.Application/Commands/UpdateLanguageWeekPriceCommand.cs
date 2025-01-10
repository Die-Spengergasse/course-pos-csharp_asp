namespace Languageweek.Application.Commands;

// TODO: Add validations
public record UpdateLanguageWeekPriceCommand(
    int Id,
    decimal PricePerPerson
);
