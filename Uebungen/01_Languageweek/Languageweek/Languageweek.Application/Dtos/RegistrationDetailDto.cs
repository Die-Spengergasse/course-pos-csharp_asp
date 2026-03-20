using System;

namespace Languageweek.Application.Dtos;

public record RegistrationDetailDto(
    int Id,
    DateTime RegisterDate,
    int StudentId,
    string StudentFirstname,
    string StudentLastname,
    string StudentEmail,
    string SchoolclassShortname,
    int LanguageweekId,
    string DestinationCity,
    DateOnly LanguageweekFrom,
    DateOnly LanguageweekTo,
    string TeacherShortname
);
