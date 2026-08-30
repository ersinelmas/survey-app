using FluentValidation;
using SurveyApp.Application.DTOs.Questions;

namespace SurveyApp.Application.Validators;

public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Soru metni boş olamaz.")
            .MaximumLength(500).WithMessage("Soru metni 500 karakterden uzun olamaz.");

        RuleFor(x => x.AnswerTemplateId)
            .NotEmpty().WithMessage("Bir cevap şablonu seçilmelidir.");
    }
}