namespace SurveyApp.Application.DTOs.Questions;

public class CreateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public Guid AnswerTemplateId { get; set; }
}