using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class SurveyRepository : ISurveyRepository
{
    private readonly SurveyDbContext _context;

    public SurveyRepository(SurveyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Survey>> GetAllAsync()
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task<Survey?> GetByIdAsync(Guid id)
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Survey survey)
    {
        await _context.Surveys.AddAsync(survey);
    }

    public void Remove(Survey survey)
    {
        _context.Surveys.Remove(survey);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
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