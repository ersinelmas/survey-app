using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<SurveyResponse>> GetBySurveyIdAsync(Guid surveyId)
    {
        return await _context.SurveyResponses
            .Include(r => r.User)
            .Include(r => r.SelectedOption)
            .Where(r => r.SurveyId == surveyId)
            .ToListAsync();
    }
}