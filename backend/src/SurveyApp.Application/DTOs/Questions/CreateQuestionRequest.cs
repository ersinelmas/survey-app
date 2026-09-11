using SurveyApp.Core.Enums;

namespace SurveyApp.Application.DTOs.Questions;

public class CreateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;
    public Guid? AnswerTemplateId { get; set; }
}