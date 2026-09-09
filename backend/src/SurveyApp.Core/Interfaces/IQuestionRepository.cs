using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IQuestionRepository : IGenericRepository<Question>
{
    Task<bool> IsUsedInAnySurveyAsync(Guid questionId);
    Task<List<Question>> GetAllForUserAsync(Guid userId);
    Task<(List<Question> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize);
}