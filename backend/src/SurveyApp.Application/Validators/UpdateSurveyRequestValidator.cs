using FluentValidation;
using SurveyApp.Application.DTOs.Surveys;

namespace SurveyApp.Application.Validators;

public class UpdateSurveyRequestValidator : AbstractValidator<UpdateSurveyRequest>
{
    public UpdateSurveyRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Anket başlığı boş olamaz.")
            .MaximumLength(300);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Bitiş tarihi, başlangıç tarihinden önce olamaz.");

        RuleFor(x => x.QuestionIds)
            .NotEmpty().WithMessage("En az bir soru seçilmelidir.");
    }
}