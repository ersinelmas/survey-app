using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IQuestionRepository : IGenericRepository<Question>
{
    Task<bool> IsUsedInAnySurveyAsync(Guid questionId);
}