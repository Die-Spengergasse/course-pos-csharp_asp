using System;

namespace Languageweek.Application.Dtos;

public record LanguageWeekDto(
    int Id, DateOnly From, DateOnly To, decimal PricePerPerson, string SchoolclassShortname,
    string DestinationCity, string TeacherShortname);
