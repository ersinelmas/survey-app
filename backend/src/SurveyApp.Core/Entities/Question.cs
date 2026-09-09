namespace SurveyApp.Core.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;

    public Guid AnswerTemplateId { get; set; }
    public AnswerTemplate AnswerTemplate { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? OwnerId { get; set; }
    public User? Owner { get; set; }
}
