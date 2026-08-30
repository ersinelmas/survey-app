using FluentValidation;
using SurveyApp.Application.DTOs.Auth;

namespace SurveyApp.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi girin.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz.");
    }
}