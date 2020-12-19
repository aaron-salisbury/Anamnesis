using Anamnesis.Core.Domains;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Anamnesis.Core.Base.ValidationAttributes
{
    class HighDateGreaterThanLowDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ChangesetQuery changesetQuery = (ChangesetQuery)validationContext.ObjectInstance;

            if (changesetQuery.HighDate == null || changesetQuery.LowDate == null || changesetQuery.HighDate >= changesetQuery.LowDate)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("High Date must be greater than or equal to Low Date.", new List<string> { validationContext.MemberName });
        }
    }
}
