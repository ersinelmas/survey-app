using SurveyApp.Core.Enums;

namespace SurveyApp.Application.DTOs.Questions;

public class UpdateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public Guid? AnswerTemplateId { get; set; }
}