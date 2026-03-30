using System.ComponentModel.DataAnnotations;

namespace Portal.Shared;

public class ValidationAttributes
{
    public class NotZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null)
                return false;

            int numericValue = (int)value;
            return numericValue != 0;
        }
    }

    public class DateGreaterThanOrEqualToToday : ValidationAttribute
    {
        public override string FormatErrorMessage(string name) => "Date value should not be a past date";

        protected override ValidationResult? IsValid(object? objValue, ValidationContext validationContext)
        {
            if (objValue is null)
                return ValidationResult.Success;

            DateTime dateValue = objValue as DateTime? ?? new DateTime();

            //alter this as needed. I am doing the date comparison if the value is not null

            if (dateValue.Date < DateTime.Now.Date)
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

            return ValidationResult.Success;
        }
    }
}
