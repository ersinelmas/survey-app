using SurveyApp.Application.DTOs.SurveyFilling;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class SurveyFillingService
{
    private readonly ISurveyAssignmentRepository _assignmentRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ISurveyResponseRepository _responseRepository;

    public SurveyFillingService(
        ISurveyAssignmentRepository assignmentRepository,
        ISurveyRepository surveyRepository,
        ISurveyResponseRepository responseRepository)
    {
        _assignmentRepository = assignmentRepository;
        _surveyRepository = surveyRepository;
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
            Questions = MapFillQuestions(assignment.Survey.SurveyQuestions)
        };
    }

    public async Task SubmitAsync(Guid userId, Guid surveyId, SubmitSurveyRequest request)
    {
        var assignment = await _assignmentRepository.GetByUserAndSurveyAsync(userId, surveyId);
        if (assignment is null)
            throw new KeyNotFoundException("Bu anket size atanmamış.");

        if (assignment.IsCompleted)
            throw new InvalidOperationException("Bu anketi zaten doldurdunuz.");

        ValidateAnswers(assignment.Survey.SurveyQuestions, request.Answers);

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

    public async Task<PublicSurveyDetailDto> GetPublicSurveyAsync(Guid surveyId, Guid? currentUserId)
    {
        var survey = await _surveyRepository.GetByIdForFillingAsync(surveyId);
        if (survey is null || !survey.IsPublic)
            throw new KeyNotFoundException("Anket bulunamadı.");

        var now = DateTime.UtcNow;
        if (!survey.IsActive || survey.StartDate > now || survey.EndDate < now)
            throw new InvalidOperationException("Bu anket şu anda aktif değil.");

        var requireLogin = survey.RequireLoginForPublicResponses;
        if (requireLogin && !currentUserId.HasValue)
        {
            return new PublicSurveyDetailDto
            {
                SurveyId = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                RequireLogin = true,
                Questions = new List<SurveyFillQuestionDto>()
            };
        }

        return new PublicSurveyDetailDto
        {
            SurveyId = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            RequireLogin = requireLogin,
            Questions = MapFillQuestions(survey.SurveyQuestions)
        };
    }

    public async Task SubmitPublicAsync(Guid surveyId, Guid? currentUserId, SubmitPublicSurveyRequest request)
    {
        var survey = await _surveyRepository.GetByIdForFillingAsync(surveyId);
        if (survey is null || !survey.IsPublic)
            throw new KeyNotFoundException("Anket bulunamadı.");

        var now = DateTime.UtcNow;
        if (!survey.IsActive || survey.StartDate > now || survey.EndDate < now)
            throw new InvalidOperationException("Bu anket şu anda aktif değil.");

        if (survey.RequireLoginForPublicResponses && !currentUserId.HasValue)
            throw new UnauthorizedAccessException("Bu anketi yanıtlamak için giriş yapmalısınız.");

        var alreadyResponded = await _responseRepository.HasRespondedAsync(surveyId, currentUserId, request.RespondentToken);
        if (alreadyResponded)
            throw new InvalidOperationException("Bu anketi zaten yanıtladınız.");

        ValidateAnswers(survey.SurveyQuestions, request.Answers);

        var responses = request.Answers.Select(a => new SurveyResponse
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            UserId = currentUserId,
            RespondentToken = currentUserId.HasValue ? null : request.RespondentToken,
            QuestionId = a.QuestionId,
            SelectedOptionId = a.SelectedOptionId
        });

        await _responseRepository.AddRangeAsync(responses);
        await _responseRepository.SaveChangesAsync();
    }

    private static void ValidateAnswers(IEnumerable<SurveyQuestion> surveyQuestions, List<SubmitAnswerDto> answers)
    {
        var validOptionIdsByQuestionId = surveyQuestions
            .ToDictionary(sq => sq.QuestionId, sq => sq.Question.AnswerTemplate.Options.Select(o => o.Id).ToHashSet());

        if (answers.Count != validOptionIdsByQuestionId.Count
            || !answers.All(a => validOptionIdsByQuestionId.ContainsKey(a.QuestionId)))
        {
            throw new ArgumentException("Anketteki tüm sorular cevaplanmalıdır.");
        }

        if (answers.Any(a => !validOptionIdsByQuestionId[a.QuestionId].Contains(a.SelectedOptionId)))
        {
            throw new ArgumentException("Seçilen şık, ilgili soruya ait değil.");
        }
    }

    private static List<SurveyFillQuestionDto> MapFillQuestions(IEnumerable<SurveyQuestion> surveyQuestions)
    {
        return surveyQuestions
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
            }).ToList();
    }
}
