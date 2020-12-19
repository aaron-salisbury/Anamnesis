using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Anamnesis.Core.Base.ValidationAttributes
{
    public class UserIsAuthorized : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Manager manager = (Manager)validationContext.ObjectInstance;

            if (string.IsNullOrEmpty(manager.UserName) || manager.ChangesetQuery.AuthorizedUsers.Contains(manager.UserName))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("User must be authorized to use source control. Spelling and capitalization counts.", new List<string> { validationContext.MemberName });
        }
    }
}
