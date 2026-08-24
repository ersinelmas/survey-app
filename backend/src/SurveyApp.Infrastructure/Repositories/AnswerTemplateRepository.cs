using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class AnswerTemplateRepository : IAnswerTemplateRepository
{
    private readonly SurveyDbContext _context;

    public AnswerTemplateRepository(SurveyDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnswerTemplate>> GetAllAsync()
    {
        return await _context.AnswerTemplates
            .Include(t => t.Options)
            .ToListAsync();
    }

    public async Task<AnswerTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.AnswerTemplates
            .Include(t => t.Options)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(AnswerTemplate template)
    {
        await _context.AnswerTemplates.AddAsync(template);
    }

    public void Remove(AnswerTemplate template)
    {
        _context.AnswerTemplates.Remove(template);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void AddOption(AnswerOption option)
    {
        _context.AnswerOptions.Add(option);
    }
}