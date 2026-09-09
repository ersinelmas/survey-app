using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyResponseRepository
{
    Task AddRangeAsync(IEnumerable<SurveyResponse> responses);
    Task SaveChangesAsync();
    Task<List<SurveyResponse>> GetBySurveyIdAsync(Guid surveyId);
    Task<bool> IsOptionUsedInAnyResponseAsync(Guid optionId);
    Task<bool> IsQuestionUsedInAnyResponseAsync(Guid questionId);
    Task<bool> HasRespondedAsync(Guid surveyId, Guid? userId, string? respondentToken);
}