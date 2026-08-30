using FluentValidation;
using SurveyApp.Application.DTOs.SurveyFilling;

namespace SurveyApp.Application.Validators;

public class SubmitSurveyRequestValidator : AbstractValidator<SubmitSurveyRequest>
{
    public SubmitSurveyRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("En az bir cevap gönderilmelidir.");

        RuleForEach(x => x.Answers)
            .ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId).NotEmpty().WithMessage("Soru ID boş olamaz.");
                answer.RuleFor(a => a.SelectedOptionId).NotEmpty().WithMessage("Seçilen şık ID boş olamaz.");
            });
    }
}