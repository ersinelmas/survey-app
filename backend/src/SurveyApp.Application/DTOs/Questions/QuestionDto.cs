namespace SurveyApp.Application.DTOs.Questions;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid AnswerTemplateId { get; set; }
    public string AnswerTemplateName { get; set; } = string.Empty;
}