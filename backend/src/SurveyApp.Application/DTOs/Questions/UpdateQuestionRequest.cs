namespace SurveyApp.Application.DTOs.Questions;

public class UpdateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public Guid AnswerTemplateId { get; set; }
}