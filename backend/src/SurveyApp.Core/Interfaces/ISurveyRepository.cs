using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyRepository : IGenericRepository<Survey>
{
    Task<Survey?> GetByIdWithResponsesAsync(Guid id);
    Task<Survey?> GetByIdForFillingAsync(Guid id);
    void RemoveSurveyQuestions(IEnumerable<SurveyQuestion> items);
    void AddSurveyQuestion(SurveyQuestion item);
    void RemoveAssignments(IEnumerable<SurveyAssignment> items);
    void AddAssignment(SurveyAssignment item);
    Task<List<Survey>> GetAllForUserAsync(Guid userId);
    Task<(List<Survey> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize);
}