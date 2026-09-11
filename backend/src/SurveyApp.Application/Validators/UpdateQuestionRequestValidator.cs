using FluentValidation;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Core.Enums;

namespace SurveyApp.Application.Validators;

public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
{
    public UpdateQuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Soru metni boş olamaz.")
            .MaximumLength(500).WithMessage("Soru metni 500 karakterden uzun olamaz.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Geçersiz soru tipi.");

        RuleFor(x => x.AnswerTemplateId)
            .NotEmpty().WithMessage("Bir cevap şablonu seçilmelidir.")
            .When(x => x.Type is QuestionType.SingleChoice or QuestionType.MultipleChoice);

        RuleFor(x => x.AnswerTemplateId)
            .Empty().WithMessage("Serbest metin sorularında cevap şablonu seçilemez.")
            .When(x => x.Type == QuestionType.FreeText);
    }
}
