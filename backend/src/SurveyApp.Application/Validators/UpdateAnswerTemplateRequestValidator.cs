using FluentValidation;
using SurveyApp.Application.DTOs.AnswerTemplates;

namespace SurveyApp.Application.Validators;

public class UpdateAnswerTemplateRequestValidator : AbstractValidator<UpdateAnswerTemplateRequest>
{
    public UpdateAnswerTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şablon adı boş olamaz.")
            .MaximumLength(200);

        RuleFor(x => x.Options)
            .Must(o => o.Count >= 2 && o.Count <= 4)
            .WithMessage("Şık sayısı 2 ile 4 arasında olmalıdır.");

        RuleFor(x => x.Options)
            .Must(o => o.Select(option => option.Text.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == o.Count)
            .WithMessage("Aynı şablonda aynı metne sahip iki şık olamaz.");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                option.RuleFor(o => o.Text)
                    .NotEmpty().WithMessage("Şık metni boş olamaz.");
            });
    }
}