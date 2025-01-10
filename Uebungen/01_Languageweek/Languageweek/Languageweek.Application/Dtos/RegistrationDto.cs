using System;

namespace Languageweek.Application.Dtos;

public record RegistrationDto(
    DateTime RegisterDate,
    int StudentId, string StudentFirstname, string StudentLastname, string StudentEmail);
