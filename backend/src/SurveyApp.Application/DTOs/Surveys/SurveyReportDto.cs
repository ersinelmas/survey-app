namespace SurveyApp.Application.DTOs.Surveys;

public class SurveyReportDto
{
    public Guid SurveyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int TotalCompleted { get; set; }
    public List<UserCompletionDto> CompletedByUsers { get; set; } = new();
    public List<UserCompletionDto> PendingUsers { get; set; } = new();
    public List<QuestionResponseSummaryDto> QuestionSummaries { get; set; } = new();
}

public class UserCompletionDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}

public class QuestionResponseSummaryDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<UserAnswerDto> UserAnswers { get; set; } = new();
}

public class UserAnswerDto
{
    public string UserEmail { get; set; } = string.Empty;
    public string SelectedOptionText { get; set; } = string.Empty;
}