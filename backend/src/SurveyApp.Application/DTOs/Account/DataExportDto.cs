using SurveyApp.Core.Enums;

namespace SurveyApp.Application.DTOs.Account;

public class DataExportDto
{
    public UserExportDto Account { get; set; } = null!;
    public List<OwnedSurveyExportDto> OwnedSurveys { get; set; } = new();
    public List<OwnedQuestionExportDto> OwnedQuestions { get; set; } = new();
    public List<OwnedAnswerTemplateExportDto> OwnedAnswerTemplates { get; set; } = new();
    public List<MyResponseExportDto> MyResponses { get; set; } = new();
    public List<MyAssignmentExportDto> MyAssignments { get; set; } = new();
}

public class UserExportDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OwnedSurveyExportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public List<string> Questions { get; set; } = new();
    public int AssignedUserCount { get; set; }
}

public class OwnedQuestionExportDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
}

public class OwnedAnswerTemplateExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

public class MyResponseExportDto
{
    public string SurveyTitle { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; }
}

public class MyAssignmentExportDto
{
    public string SurveyTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
