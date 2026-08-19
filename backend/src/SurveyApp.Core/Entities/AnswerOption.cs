namespace SurveyApp.Core.Entities;

public class AnswerOption
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }

    public Guid AnswerTemplateId { get; set; }
    public AnswerTemplate AnswerTemplate { get; set; } = null!;
}