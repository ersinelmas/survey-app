namespace SurveyApp.Core.Entities;

public class SurveyResponse
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? RespondentToken { get; set; }

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public Guid? SelectedOptionId { get; set; }
    public AnswerOption? SelectedOption { get; set; }

    public string? TextValue { get; set; }

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}
