using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IAnswerTemplateRepository : IGenericRepository<AnswerTemplate>
{
    void AddOption(AnswerOption option);
    Task<bool> IsUsedInAnyQuestionAsync(Guid templateId);
    Task<List<AnswerTemplate>> GetAllForUserAsync(Guid userId);
    Task<(List<AnswerTemplate> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize);
}