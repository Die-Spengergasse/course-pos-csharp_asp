using Eventmanager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Eventmanager.Application.Commands;

public record UpdateContingentCmd(
    [Range(1,int.MaxValue, ErrorMessage = "Invalid id.")]
    int Id,
    [Range(1,int.MaxValue, ErrorMessage = "Invalid show id.")]
    int ShowId,
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid contingent type.")]
    string ContingentType,
    [Range(1,9999)]
    int AvailableTickets,
    long Version) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(typeof(ContingentType), ContingentType))
        {
            yield return new ValidationResult(
                "Invalid value for contingent type",
                new string[] { nameof(ContingentType) });
        }
    }
}
