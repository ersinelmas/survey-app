namespace SurveyApp.Application.DTOs.Surveys;

public class SurveyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<SurveyQuestionDto> Questions { get; set; } = new();
    public List<SurveyAssignedUserDto> AssignedUsers { get; set; } = new();
}

public class SurveyQuestionDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class SurveyAssignedUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}