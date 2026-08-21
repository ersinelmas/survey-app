namespace SurveyApp.Application.DTOs.SurveyFilling;

public class SurveyFillDetailDto
{
    public Guid SurveyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<SurveyFillQuestionDto> Questions { get; set; } = new();
}

public class SurveyFillQuestionDto
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<SurveyFillOptionDto> Options { get; set; } = new();
}

public class SurveyFillOptionDto
{
    public Guid OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
}