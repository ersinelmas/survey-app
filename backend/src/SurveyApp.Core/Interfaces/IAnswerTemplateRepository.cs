using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IAnswerTemplateRepository
{
    Task<List<AnswerTemplate>> GetAllAsync();
    Task<AnswerTemplate?> GetByIdAsync(Guid id);
    Task AddAsync(AnswerTemplate template);
    void Remove(AnswerTemplate template);
    Task SaveChangesAsync();
    void AddOption(AnswerOption option);
    Task<bool> IsUsedInAnyQuestionAsync(Guid templateId);
}