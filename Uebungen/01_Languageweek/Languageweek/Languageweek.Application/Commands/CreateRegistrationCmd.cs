using System;

namespace Languageweek.Application.Commands;

// TODO: Add validations.
public record CreateRegistrationCmd(
    int LanguageweekId,
    int StudentId,
    DateTime RegisterDate);
