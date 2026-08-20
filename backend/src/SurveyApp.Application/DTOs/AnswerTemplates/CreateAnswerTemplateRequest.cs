namespace SurveyApp.Application.DTOs.AnswerTemplates;

public class CreateAnswerTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public List<CreateAnswerOptionRequest> Options { get; set; } = new();
}

public class CreateAnswerOptionRequest
{
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}