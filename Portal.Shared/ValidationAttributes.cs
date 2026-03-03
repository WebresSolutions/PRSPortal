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
}
