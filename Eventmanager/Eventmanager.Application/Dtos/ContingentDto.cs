using System;

namespace Eventmanager.Application.Dtos;

public record ContingentDto(
    int Id, int ShowId, DateTime ShowDate, string EventName,
    string ContingentType, int AvailableTickets);