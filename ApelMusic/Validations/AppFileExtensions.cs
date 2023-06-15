using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Validations
{
    public class AppFileExtensions : ValidationAttribute
    {
        public string[] AllowMimeTypes { get; set; } = new string[5];

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && value != null)
            {
                if (!AllowMimeTypes.Contains(file.ContentType))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}