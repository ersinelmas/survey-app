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

    public new async Task<(List<Survey> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
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

    public async Task<Survey?> GetByIdForFillingAsync(Guid id)
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question).ThenInclude(q => q.AnswerTemplate).ThenInclude(at => at.Options)
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

    public async Task<List<Survey>> GetAllForUserAsync(Guid userId)
    {
        return await _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .Where(s => s.OwnerId == userId)
            .ToListAsync();
    }

    public async Task<(List<Survey> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.Surveys
            .Include(s => s.SurveyQuestions).ThenInclude(sq => sq.Question)
            .Include(s => s.Assignments).ThenInclude(a => a.User)
            .Where(s => s.OwnerId == userId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt).ThenBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}