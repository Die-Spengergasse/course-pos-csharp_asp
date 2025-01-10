using System;
using System.Collections.Generic;

namespace Languageweek.Application.Dtos;

public record LanguageWeekWithRegistrationsDto(
    int Id, DateOnly From, DateOnly To, decimal PricePerPerson, string SchoolclassShortname,
    string DestinationCity, string TeacherShortname,
    List<RegistrationDto> Registrations);
