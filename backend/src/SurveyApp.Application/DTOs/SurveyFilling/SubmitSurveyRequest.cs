namespace SurveyApp.Application.DTOs.SurveyFilling;

public class SubmitSurveyRequest
{
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
    public string? TextValue { get; set; }
}
