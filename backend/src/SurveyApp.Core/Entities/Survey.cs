namespace SurveyApp.Core.Entities;

public class Survey
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? OwnerId { get; set; }
    public User? Owner { get; set; }

    public ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();
    public ICollection<SurveyAssignment> Assignments { get; set; } = new List<SurveyAssignment>();
}
