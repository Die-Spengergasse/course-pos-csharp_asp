using System;

namespace Fitnesscenter.Application.Services;

[Serializable]
public class FitnessServiceException : Exception
{
    public FitnessServiceException()
    {
    }

    public FitnessServiceException(string? message) : base(message)
    {
    }

    public FitnessServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}