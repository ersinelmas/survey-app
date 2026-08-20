namespace SurveyApp.Application.DTOs.AnswerTemplates;

public class AnswerTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<AnswerOptionDto> Options { get; set; } = new();
}

public class AnswerOptionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}