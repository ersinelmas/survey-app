namespace SurveyApp.Application.DTOs.SurveyFilling;

public class SubmitPublicSurveyRequest
{
    public List<SubmitAnswerDto> Answers { get; set; } = new();
    public string? RespondentToken { get; set; }
}
