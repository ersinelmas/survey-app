namespace SurveyApp.Application.DTOs.SurveyFilling;

public class AssignedSurveyDto
{
    public Guid SurveyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
}