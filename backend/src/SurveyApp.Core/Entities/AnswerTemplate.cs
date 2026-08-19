namespace SurveyApp.Core.Entities;

public class AnswerTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
}