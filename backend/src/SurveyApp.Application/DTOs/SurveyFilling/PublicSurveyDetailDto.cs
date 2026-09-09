namespace SurveyApp.Application.DTOs.SurveyFilling;

public class PublicSurveyDetailDto
{
    public Guid SurveyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequireLogin { get; set; }
    public List<SurveyFillQuestionDto> Questions { get; set; } = new();
}
