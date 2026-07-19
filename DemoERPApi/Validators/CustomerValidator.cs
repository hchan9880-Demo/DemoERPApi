using FluentValidation;
using DemoERPApi.Models;

namespace DemoERPApi.Validators
{
    public class CustomerValidator : AbstractValidator<CustomersDto>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email is required.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First Name is required.");
        }
    }
}