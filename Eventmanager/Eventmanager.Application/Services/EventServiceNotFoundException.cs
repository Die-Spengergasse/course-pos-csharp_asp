using System;

namespace Eventmanager.Application.Services;

public class EventServiceNotFoundException : EventServiceException
{
    public EventServiceNotFoundException()
    {
    }

    public EventServiceNotFoundException(string? message) : base(message)
    {
    }

    public EventServiceNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}