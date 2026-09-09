using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
{
    public QuestionRepository(SurveyDbContext context) : base(context)
    {
    }

    public new async Task<List<Question>> GetAllAsync()
    {
        return await _context.Questions
            .Include(q => q.AnswerTemplate)
            .ToListAsync();
    }

    public new async Task<(List<Question> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Questions.Include(q => q.AnswerTemplate);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(q => q.CreatedAt).ThenBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public new async Task<Question?> GetByIdAsync(Guid id)
    {
        return await _context.Questions
            .Include(q => q.AnswerTemplate)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<bool> IsUsedInAnySurveyAsync(Guid questionId)
    {
        return await _context.SurveyQuestions.AnyAsync(sq => sq.QuestionId == questionId);
    }

    public async Task<List<Question>> GetAllForUserAsync(Guid userId)
    {
        return await _context.Questions
            .Include(q => q.AnswerTemplate)
            .Where(q => q.OwnerId == null || q.OwnerId == userId)
            .ToListAsync();
    }

    public async Task<(List<Question> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.Questions
            .Include(q => q.AnswerTemplate)
            .Where(q => q.OwnerId == null || q.OwnerId == userId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(q => q.CreatedAt).ThenBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}