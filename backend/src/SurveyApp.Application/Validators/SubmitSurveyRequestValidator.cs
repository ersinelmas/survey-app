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
            .SetValidator(new SubmitAnswerDtoValidator());
    }
}

public class SubmitAnswerDtoValidator : AbstractValidator<SubmitAnswerDto>
{
    public SubmitAnswerDtoValidator()
    {
        RuleFor(a => a.QuestionId).NotEmpty().WithMessage("Soru ID boş olamaz.");

        RuleFor(a => a)
            .Must(a => a.SelectedOptionIds.Count > 0 || !string.IsNullOrWhiteSpace(a.TextValue))
            .WithMessage("Bir cevap girilmelidir.");

        RuleFor(a => a.TextValue)
            .MaximumLength(1000).WithMessage("Cevap metni 1000 karakterden uzun olamaz.");
    }
}
