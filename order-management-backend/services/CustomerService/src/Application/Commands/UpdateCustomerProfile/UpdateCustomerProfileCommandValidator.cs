using FluentValidation;

namespace CustomerService.Application.Commands.UpdateCustomerProfile;

/// <summary>
/// Validador para el comando de actualización de perfil de cliente
/// </summary>
public class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
{
    public UpdateCustomerProfileCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth must be realistic")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender must be a valid value")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.Preferences)
            .MaximumLength(1000).WithMessage("Preferences must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Preferences));
    }
}