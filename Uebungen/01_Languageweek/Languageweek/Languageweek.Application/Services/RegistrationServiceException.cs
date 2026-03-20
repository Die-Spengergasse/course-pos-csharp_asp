using System;

namespace Languageweek.Application.Services
{
    [Serializable]
    public class RegistrationServiceException : Exception
    {
        public RegistrationServiceException()
        {
        }

        public RegistrationServiceException(string? message) : base(message)
        {
        }

        public RegistrationServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}