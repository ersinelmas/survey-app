using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class SurveyAssignmentRepository : ISurveyAssignmentRepository
{
    private readonly SurveyDbContext _context;

    public SurveyAssignmentRepository(SurveyDbContext context)
    {
        _context = context;
    }

    public async Task<List<SurveyAssignment>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SurveyAssignments
            .Include(a => a.Survey)
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<SurveyAssignment?> GetByUserAndSurveyAsync(Guid userId, Guid surveyId)
    {
        return await _context.SurveyAssignments
            .Include(a => a.Survey)
                .ThenInclude(s => s.SurveyQuestions)
                    .ThenInclude(sq => sq.Question)
                        .ThenInclude(q => q.AnswerTemplate)
                            .ThenInclude(at => at.Options)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.SurveyId == surveyId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}