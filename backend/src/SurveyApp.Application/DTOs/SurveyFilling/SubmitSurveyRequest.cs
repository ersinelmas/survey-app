namespace SurveyApp.Application.DTOs.SurveyFilling;

public class SubmitSurveyRequest
{
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}