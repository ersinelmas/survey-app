using SurveyApp.Application.DTOs.SurveyFilling;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Enums;
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

        var alreadyRespondedElsewhere = await _responseRepository.HasRespondedAsync(surveyId, userId, respondentToken: null);
        if (alreadyRespondedElsewhere)
        {
            assignment.IsCompleted = true;
            assignment.CompletedAt = DateTime.UtcNow;
            await _assignmentRepository.SaveChangesAsync();
            throw new InvalidOperationException("Bu anketi zaten doldurdunuz.");
        }

        ValidateAnswers(assignment.Survey.SurveyQuestions, request.Answers);

        var responses = BuildResponses(request.Answers, r =>
        {
            r.SurveyId = surveyId;
            r.UserId = userId;
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

        var responses = BuildResponses(request.Answers, r =>
        {
            r.SurveyId = surveyId;
            r.UserId = currentUserId;
            r.RespondentToken = currentUserId.HasValue ? null : request.RespondentToken;
        });

        await _responseRepository.AddRangeAsync(responses);

        if (currentUserId.HasValue)
        {
            var assignment = await _assignmentRepository.GetByUserAndSurveyAsync(currentUserId.Value, surveyId);
            if (assignment is not null && !assignment.IsCompleted)
            {
                assignment.IsCompleted = true;
                assignment.CompletedAt = DateTime.UtcNow;
            }
        }

        await _responseRepository.SaveChangesAsync();
    }

    private static void ValidateAnswers(IEnumerable<SurveyQuestion> surveyQuestions, List<SubmitAnswerDto> answers)
    {
        var questionsById = surveyQuestions.ToDictionary(sq => sq.QuestionId, sq => sq.Question);
        var answersByQuestionId = answers.ToDictionary(a => a.QuestionId);

        if (answers.Count != questionsById.Count
            || !answers.All(a => questionsById.ContainsKey(a.QuestionId)))
        {
            throw new ArgumentException("Anketteki tüm sorular cevaplanmalıdır.");
        }

        foreach (var (questionId, question) in questionsById)
        {
            var answer = answersByQuestionId[questionId];

            switch (question.Type)
            {
                case QuestionType.SingleChoice:
                    if (answer.SelectedOptionIds.Count != 1)
                        throw new ArgumentException("Tekli seçim sorularında tam olarak bir şık seçilmelidir.");
                    ValidateOptionsBelongToQuestion(question, answer.SelectedOptionIds);
                    break;

                case QuestionType.MultipleChoice:
                    if (answer.SelectedOptionIds.Count == 0)
                        throw new ArgumentException("Çoktan seçmeli sorularda en az bir şık seçilmelidir.");
                    if (answer.SelectedOptionIds.Distinct().Count() != answer.SelectedOptionIds.Count)
                        throw new ArgumentException("Aynı şık birden fazla kez seçilemez.");
                    ValidateOptionsBelongToQuestion(question, answer.SelectedOptionIds);
                    break;

                case QuestionType.FreeText:
                    if (string.IsNullOrWhiteSpace(answer.TextValue))
                        throw new ArgumentException("Serbest metin sorusu boş bırakılamaz.");
                    break;
            }
        }
    }

    private static void ValidateOptionsBelongToQuestion(Question question, IEnumerable<Guid> selectedOptionIds)
    {
        var validOptionIds = question.AnswerTemplate!.Options.Select(o => o.Id).ToHashSet();
        if (selectedOptionIds.Any(id => !validOptionIds.Contains(id)))
            throw new ArgumentException("Seçilen şık, ilgili soruya ait değil.");
    }

    private static IEnumerable<SurveyResponse> BuildResponses(List<SubmitAnswerDto> answers, Action<SurveyResponse> configure)
    {
        foreach (var answer in answers)
        {
            if (answer.SelectedOptionIds.Count > 0)
            {
                foreach (var optionId in answer.SelectedOptionIds)
                {
                    var response = new SurveyResponse
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = answer.QuestionId,
                        SelectedOptionId = optionId
                    };
                    configure(response);
                    yield return response;
                }
            }
            else
            {
                var response = new SurveyResponse
                {
                    Id = Guid.NewGuid(),
                    QuestionId = answer.QuestionId,
                    TextValue = answer.TextValue
                };
                configure(response);
                yield return response;
            }
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
                Type = sq.Question.Type,
                Options = sq.Question.AnswerTemplate?.Options
                    .OrderBy(o => o.Order)
                    .Select(o => new SurveyFillOptionDto
                    {
                        OptionId = o.Id,
                        Text = o.Text
                    }).ToList() ?? new List<SurveyFillOptionDto>()
            }).ToList();
    }
}
