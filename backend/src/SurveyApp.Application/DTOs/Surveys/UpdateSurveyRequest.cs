namespace SurveyApp.Application.DTOs.Surveys;

public class UpdateSurveyRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> QuestionIds { get; set; } = new();
    public List<Guid> AssignedUserIds { get; set; } = new();
}