using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IAnswerTemplateRepository : IGenericRepository<AnswerTemplate>
{
    void AddOption(AnswerOption option);
    Task<bool> IsUsedInAnyQuestionAsync(Guid templateId);
}