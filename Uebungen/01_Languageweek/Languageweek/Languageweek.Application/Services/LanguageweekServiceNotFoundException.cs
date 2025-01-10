using System;

namespace Languageweek.Application.Services;

public class LanguageweekServiceNotFoundException : LanguageweekServiceException
{
    public LanguageweekServiceNotFoundException()
    {
    }

    public LanguageweekServiceNotFoundException(string? message) : base(message)
    {
    }

    public LanguageweekServiceNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}