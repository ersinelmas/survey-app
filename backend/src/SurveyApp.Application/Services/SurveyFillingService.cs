using SurveyApp.Application.DTOs.SurveyFilling;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class SurveyFillingService
{
    private readonly ISurveyAssignmentRepository _assignmentRepository;
    private readonly ISurveyResponseRepository _responseRepository;

    public SurveyFillingService(
        ISurveyAssignmentRepository assignmentRepository,
        ISurveyResponseRepository responseRepository)
    {
        _assignmentRepository = assignmentRepository;
        _responseRepository = responseRepository;
    }

    public async Task<List<AssignedSurveyDto>> GetMyActiveSurveysAsync(Guid userId)
    {
        var assignments = await _assignmentRepository.GetByUserIdAsync(userId);
        var now = DateTime.UtcNow;

        return assignments
            .Where(a => !a.IsCompleted
                && a.Survey.IsActive
                && a.Survey.StartDate <= now
                && a.Survey.EndDate >= now)
            .Select(a => new AssignedSurveyDto
            {
                SurveyId = a.Survey.Id,
                Title = a.Survey.Title,
                Description = a.Survey.Description,
                EndDate = a.Survey.EndDate
            })
            .ToList();
    }

    public async Task<SurveyFillDetailDto> GetSurveyToFillAsync(Guid userId, Guid surveyId)
    {
        var assignment = await _assignmentRepository.GetByUserAndSurveyAsync(userId, surveyId);
        if (assignment is null)
            throw new KeyNotFoundException("Bu anket size atanmamış.");

        if (assignment.IsCompleted)
            throw new InvalidOperationException("Bu anketi zaten doldurdunuz.");

        var now = DateTime.UtcNow;
        if (assignment.Survey.StartDate > now || assignment.Survey.EndDate < now)
            throw new InvalidOperationException("Bu anket şu anda aktif değil.");

        return new SurveyFillDetailDto
        {
            SurveyId = assignment.Survey.Id,
            Title = assignment.Survey.Title,
            Description = assignment.Survey.Description,
            Questions = assignment.Survey.SurveyQuestions
                .OrderBy(sq => sq.Order)
                .Select(sq => new SurveyFillQuestionDto
                {
                    QuestionId = sq.Question.Id,
                    Text = sq.Question.Text,
                    Options = sq.Question.AnswerTemplate.Options
                        .OrderBy(o => o.Order)
                        .Select(o => new SurveyFillOptionDto
                        {
                            OptionId = o.Id,
                            Text = o.Text
                        }).ToList()
                }).ToList()
        };
    }

    public async Task SubmitAsync(Guid userId, Guid surveyId, SubmitSurveyRequest request)
    {
        var assignment = await _assignmentRepository.GetByUserAndSurveyAsync(userId, surveyId);
        if (assignment is null)
            throw new KeyNotFoundException("Bu anket size atanmamış.");

        if (assignment.IsCompleted)
            throw new InvalidOperationException("Bu anketi zaten doldurdunuz.");

        var validQuestionIds = assignment.Survey.SurveyQuestions
            .Select(sq => sq.QuestionId)
            .ToHashSet();

        if (request.Answers.Count != validQuestionIds.Count
            || !request.Answers.All(a => validQuestionIds.Contains(a.QuestionId)))
        {
            throw new ArgumentException("Anketteki tüm sorular cevaplanmalıdır.");
        }

        var responses = request.Answers.Select(a => new SurveyResponse
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            UserId = userId,
            QuestionId = a.QuestionId,
            SelectedOptionId = a.SelectedOptionId
        });

        await _responseRepository.AddRangeAsync(responses);

        assignment.IsCompleted = true;
        assignment.CompletedAt = DateTime.UtcNow;

        await _responseRepository.SaveChangesAsync();
    }
}