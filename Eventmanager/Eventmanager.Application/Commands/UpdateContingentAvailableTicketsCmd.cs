using System;
using System.ComponentModel.DataAnnotations;

namespace Eventmanager.Application.Commands;

public record UpdateContingentAvailableTicketsCmd(
    [Range(1,9999,ErrorMessage = "Invalid id.")]
    int Id,
    [Range(1,9999)]
    int AvailableTickets);
