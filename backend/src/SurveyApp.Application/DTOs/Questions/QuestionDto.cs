using SurveyApp.Core.Enums;

namespace SurveyApp.Application.DTOs.Questions;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public Guid? AnswerTemplateId { get; set; }
    public string? AnswerTemplateName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsMine { get; set; }
}