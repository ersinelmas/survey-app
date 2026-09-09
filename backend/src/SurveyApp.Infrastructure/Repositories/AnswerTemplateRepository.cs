using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class AnswerTemplateRepository : GenericRepository<AnswerTemplate>, IAnswerTemplateRepository
{
    public AnswerTemplateRepository(SurveyDbContext context) : base(context)
    {
    }

    public new async Task<List<AnswerTemplate>> GetAllAsync()
    {
        return await _context.AnswerTemplates
            .Include(t => t.Options)
            .ToListAsync();
    }

    public new async Task<(List<AnswerTemplate> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.AnswerTemplates.Include(t => t.Options);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public new async Task<AnswerTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.AnswerTemplates
            .Include(t => t.Options)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public void AddOption(AnswerOption option)
    {
        _context.AnswerOptions.Add(option);
    }

    public async Task<bool> IsUsedInAnyQuestionAsync(Guid templateId)
    {
        return await _context.Questions.AnyAsync(q => q.AnswerTemplateId == templateId);
    }

    public async Task<List<AnswerTemplate>> GetAllForUserAsync(Guid userId)
    {
        return await _context.AnswerTemplates
            .Include(t => t.Options)
            .Where(t => t.OwnerId == null || t.OwnerId == userId)
            .ToListAsync();
    }

    public async Task<(List<AnswerTemplate> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.AnswerTemplates
            .Include(t => t.Options)
            .Where(t => t.OwnerId == null || t.OwnerId == userId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}