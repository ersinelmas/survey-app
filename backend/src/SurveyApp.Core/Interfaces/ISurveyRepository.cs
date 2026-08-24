using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyRepository
{
    Task<List<Survey>> GetAllAsync();
    Task<Survey?> GetByIdAsync(Guid id);
    Task AddAsync(Survey survey);
    void Remove(Survey survey);
    Task SaveChangesAsync();
    Task<Survey?> GetByIdWithResponsesAsync(Guid id);
    void RemoveSurveyQuestions(IEnumerable<SurveyQuestion> items);
    void AddSurveyQuestion(SurveyQuestion item);
    void RemoveAssignments(IEnumerable<SurveyAssignment> items);
    void AddAssignment(SurveyAssignment item);
}