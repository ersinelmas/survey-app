using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllAsync();
    Task<Question?> GetByIdAsync(Guid id);
    Task AddAsync(Question question);
    void Remove(Question question);
    Task SaveChangesAsync();
    Task<bool> IsUsedInAnySurveyAsync(Guid questionId);
}