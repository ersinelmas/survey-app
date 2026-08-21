using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyAssignmentRepository
{
    Task<List<SurveyAssignment>> GetByUserIdAsync(Guid userId);
    Task<SurveyAssignment?> GetByUserAndSurveyAsync(Guid userId, Guid surveyId);
    Task SaveChangesAsync();
}