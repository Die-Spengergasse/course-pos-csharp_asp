using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspShowcase.Commands
{
    public record TaskCmd(
        Guid Guid,
        [StringLength(16, MinimumLength = 1, ErrorMessage = "Ungültiges Subject")] string Subject,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Ungültiger Title")] string Title,
        Guid TeamGuid,
        Guid TeacherGuid,
        DateTime ExpirationDate,
        [Range(1, 999, ErrorMessage = "Ungültige maximale Punkteanzahl")] int? MaxPoints) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpirationDate < DateTime.Now)
                yield return new ValidationResult(
                    "Das Expiration Date muss in der Zukunft liegen",
                    new string[] { nameof(ExpirationDate) });
        }
    }
}