using FluentValidation;
using SurveyApp.Application.DTOs.Surveys;

namespace SurveyApp.Application.Validators;

public class CreateSurveyRequestValidator : AbstractValidator<CreateSurveyRequest>
{
    public CreateSurveyRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Anket başlığı boş olamaz.")
            .MaximumLength(300);

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow.Date)
            .WithMessage("Başlangıç tarihi bugünden önce olamaz.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Bitiş tarihi, başlangıç tarihinden önce olamaz.")
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow.Date)
            .WithMessage("Bitiş tarihi bugünden önce olamaz.");

        RuleFor(x => x.QuestionIds)
            .NotEmpty().WithMessage("En az bir soru seçilmelidir.");
    }
}