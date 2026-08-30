using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyRepository : IGenericRepository<Survey>
{
    Task<Survey?> GetByIdWithResponsesAsync(Guid id);
    void RemoveSurveyQuestions(IEnumerable<SurveyQuestion> items);
    void AddSurveyQuestion(SurveyQuestion item);
    void RemoveAssignments(IEnumerable<SurveyAssignment> items);
    void AddAssignment(SurveyAssignment item);
}