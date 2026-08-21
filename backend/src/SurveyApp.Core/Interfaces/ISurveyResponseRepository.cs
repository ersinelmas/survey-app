using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyResponseRepository
{
    Task AddRangeAsync(IEnumerable<SurveyResponse> responses);
    Task SaveChangesAsync();
}