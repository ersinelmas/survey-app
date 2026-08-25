using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly SurveyDbContext _context;

    public QuestionRepository(SurveyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Question>> GetAllAsync()
    {
        return await _context.Questions
            .Include(q => q.AnswerTemplate)
            .ToListAsync();
    }

    public async Task<Question?> GetByIdAsync(Guid id)
    {
        return await _context.Questions
            .Include(q => q.AnswerTemplate)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task AddAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
    }

    public void Remove(Question question)
    {
        _context.Questions.Remove(question);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUsedInAnySurveyAsync(Guid questionId)
    {
        return await _context.SurveyQuestions.AnyAsync(sq => sq.QuestionId == questionId);
    }
}