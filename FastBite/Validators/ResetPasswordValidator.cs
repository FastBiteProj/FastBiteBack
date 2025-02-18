using FastBite.Data.DTOS;
using FluentValidation;
using System.Text.RegularExpressions;

namespace FastBite.Validators;
public class ResetPasswordValidator : AbstractValidator<ResetPasswordWithCodeDTO>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.NewPassword)
            .Matches(RegexPatterns.passwordPattern)
            .NotEmpty().WithMessage("Must Be Not Empty")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(50).WithMessage("Password must be at most 50 characters long")
            .WithMessage("Password must contain at least one number")
            .WithMessage("Password must not contain spaces");

        RuleFor(x => x.ConfirmNewPassword)
            .Matches(RegexPatterns.passwordPattern)
            .NotEmpty().WithMessage("Must Be Not Empty")
            .Equal(x => x.NewPassword).WithMessage("Passwords must match");
    }
}
