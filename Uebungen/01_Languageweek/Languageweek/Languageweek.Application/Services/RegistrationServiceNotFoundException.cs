using System;

namespace Languageweek.Application.Services
{
    [Serializable]
    public class RegistrationServiceNotFoundException : RegistrationServiceException
    {
        public RegistrationServiceNotFoundException()
        {
        }

        public RegistrationServiceNotFoundException(string? message) : base(message)
        {
        }

        public RegistrationServiceNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
