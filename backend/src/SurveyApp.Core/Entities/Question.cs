using SurveyApp.Core.Enums;

namespace SurveyApp.Core.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;

    public Guid? AnswerTemplateId { get; set; }
    public AnswerTemplate? AnswerTemplate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? OwnerId { get; set; }
    public User? Owner { get; set; }
}
