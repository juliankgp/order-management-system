using FluentValidation;

namespace LoggingService.Application.Commands.CreateLog;

public class CreateLogCommandValidator : AbstractValidator<CreateLogCommand>
{
    public CreateLogCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(2000)
            .WithMessage("Message cannot exceed 2000 characters");

        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("ServiceName is required")
            .MaximumLength(100)
            .WithMessage("ServiceName cannot exceed 100 characters");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters");

        RuleFor(x => x.Level)
            .IsInEnum()
            .WithMessage("Invalid log level");

        RuleFor(x => x.CorrelationId)
            .MaximumLength(100)
            .WithMessage("CorrelationId cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CorrelationId));

        RuleFor(x => x.Exception)
            .MaximumLength(4000)
            .WithMessage("Exception cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.Exception));

        RuleFor(x => x.StackTrace)
            .MaximumLength(8000)
            .WithMessage("StackTrace cannot exceed 8000 characters")
            .When(x => !string.IsNullOrEmpty(x.StackTrace));

        RuleFor(x => x.Properties)
            .MaximumLength(4000)
            .WithMessage("Properties cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.Properties));

        RuleFor(x => x.MachineName)
            .MaximumLength(100)
            .WithMessage("MachineName cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.MachineName));

        RuleFor(x => x.Environment)
            .MaximumLength(50)
            .WithMessage("Environment cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Environment));

        RuleFor(x => x.ApplicationVersion)
            .MaximumLength(50)
            .WithMessage("ApplicationVersion cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ApplicationVersion));
    }
}