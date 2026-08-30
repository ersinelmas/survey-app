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
}