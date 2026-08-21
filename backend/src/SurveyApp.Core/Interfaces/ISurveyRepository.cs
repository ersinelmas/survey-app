using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyRepository
{
    Task<List<Survey>> GetAllAsync();
    Task<Survey?> GetByIdAsync(Guid id);
    Task AddAsync(Survey survey);
    void Remove(Survey survey);
    Task SaveChangesAsync();
}