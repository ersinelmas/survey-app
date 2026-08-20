namespace SurveyApp.Application.DTOs.AnswerTemplates;

public class UpdateAnswerTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public List<UpdateAnswerOptionRequest> Options { get; set; } = new();
}

public class UpdateAnswerOptionRequest
{
    public Guid? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}