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
}