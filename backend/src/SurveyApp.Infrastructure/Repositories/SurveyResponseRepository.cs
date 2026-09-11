using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SurveyApp.Infrastructure.Repositories;

public class SurveyResponseRepository : ISurveyResponseRepository
{
    private readonly SurveyDbContext _context;

    public SurveyResponseRepository(SurveyDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<SurveyResponse> responses)
    {
        await _context.SurveyResponses.AddRangeAsync(responses);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<SurveyResponse>> GetBySurveyIdAsync(Guid surveyId)
    {
        return await _context.SurveyResponses
            .Include(r => r.User)
            .Include(r => r.SelectedOption)
            .Include(r => r.Question)
            .Where(r => r.SurveyId == surveyId)
            .ToListAsync();
    }

    public async Task<List<SurveyResponse>> GetByUserIdAsync(Guid userId)
    {
        return await _context.SurveyResponses
            .Include(r => r.Survey)
            .Include(r => r.Question)
            .Include(r => r.SelectedOption)
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsOptionUsedInAnyResponseAsync(Guid optionId)
    {
        return await _context.SurveyResponses.AnyAsync(r => r.SelectedOptionId == optionId);
    }

    public async Task<bool> IsQuestionUsedInAnyResponseAsync(Guid questionId)
    {
        return await _context.SurveyResponses.AnyAsync(r => r.QuestionId == questionId);
    }

    public async Task<bool> HasRespondedAsync(Guid surveyId, Guid? userId, string? respondentToken)
    {
        if (userId.HasValue)
            return await _context.SurveyResponses.AnyAsync(r => r.SurveyId == surveyId && r.UserId == userId.Value);

        if (string.IsNullOrEmpty(respondentToken))
            return false;

        return await _context.SurveyResponses.AnyAsync(r => r.SurveyId == surveyId && r.RespondentToken == respondentToken);
    }
}