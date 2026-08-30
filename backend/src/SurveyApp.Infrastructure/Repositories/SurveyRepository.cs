using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class SurveyRepository : GenericRepository<Survey>, ISurveyRepository
{
    public SurveyRepository(SurveyDbContext context) : base(context)
    {
    }

    public new async Task<List<Survey>> GetAllAsync()
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .ToListAsync();
    }

    public new async Task<Survey?> GetByIdAsync(Guid id)
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Survey?> GetByIdWithResponsesAsync(Guid id)
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public void RemoveSurveyQuestions(IEnumerable<SurveyQuestion> items)
    {
        _context.SurveyQuestions.RemoveRange(items);
    }

    public void AddSurveyQuestion(SurveyQuestion item)
    {
        _context.SurveyQuestions.Add(item);
    }

    public void RemoveAssignments(IEnumerable<SurveyAssignment> items)
    {
        _context.SurveyAssignments.RemoveRange(items);
    }

    public void AddAssignment(SurveyAssignment item)
    {
        _context.SurveyAssignments.Add(item);
    }
}