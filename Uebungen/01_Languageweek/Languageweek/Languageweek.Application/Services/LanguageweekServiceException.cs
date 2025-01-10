using System;

namespace Languageweek.Application.Services;

[Serializable]
public class LanguageweekServiceException : Exception
{
    public LanguageweekServiceException()
    {
    }

    public LanguageweekServiceException(string? message) : base(message)
    {
    }

    public LanguageweekServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
