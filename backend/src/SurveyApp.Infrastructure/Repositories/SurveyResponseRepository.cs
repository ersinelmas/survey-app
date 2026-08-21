using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class SurveyResponseRepository : ISurveyResponseRepository
{
    private readonly SurveyDbContext _context;

    public SurveyResponseRepository(SurveyDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<SurveyResponse> responses)
    {
        await _context.SurveyResponses.AddRangeAsync(responses);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}