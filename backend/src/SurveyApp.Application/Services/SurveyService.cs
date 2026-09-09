using SurveyApp.Application.DTOs.Common;
using SurveyApp.Application.DTOs.Surveys;
using SurveyApp.Application.Exceptions;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class SurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISurveyResponseRepository _responseRepository;

    public SurveyService(
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        IUserRepository userRepository,
        ISurveyResponseRepository responseRepository)
    {
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _userRepository = userRepository;
        _responseRepository = responseRepository;
    }

    public async Task<List<SurveyDto>> GetAllAsync(Guid currentUserId)
    {
        var surveys = await _surveyRepository.GetAllForUserAsync(currentUserId);
        return surveys.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<SurveyDto>> GetPagedAsync(int page, int pageSize, Guid currentUserId)
    {
        var (items, totalCount) = await _surveyRepository.GetPagedForUserAsync(currentUserId, page, pageSize);
        return new PagedResult<SurveyDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SurveyDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey is null || !IsVisibleTo(survey, currentUserId, isAdmin))
            throw new KeyNotFoundException("Anket bulunamadı.");

        return MapToDto(survey);
    }

    public async Task<SurveyDto> CreateAsync(CreateSurveyRequest request, Guid currentUserId)
    {
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            OwnerId = currentUserId
        };

        await AttachQuestions(survey, request.QuestionIds, currentUserId);
        await AttachAssignments(survey, request.AssignedUserIds, new Dictionary<Guid, DateTime>());

        await _surveyRepository.AddAsync(survey);
        await _surveyRepository.SaveChangesAsync();

        return await GetByIdAsync(survey.Id, currentUserId, isAdmin: false);
    }

    public async Task<SurveyDto> UpdateAsync(Guid id, UpdateSurveyRequest request, Guid currentUserId, bool isAdmin)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey is null || !IsVisibleTo(survey, currentUserId, isAdmin))
            throw new KeyNotFoundException("Anket bulunamadı.");

        if (!CanModify(survey, currentUserId, isAdmin))
            throw new ForbiddenAccessException("Bu anketi düzenleme yetkiniz yok.");

        survey.Title = request.Title;
        survey.Description = request.Description;
        survey.StartDate = request.StartDate;
        survey.EndDate = request.EndDate;
        survey.IsActive = request.IsActive;

        _surveyRepository.RemoveSurveyQuestions(survey.SurveyQuestions.ToList());
        await AttachQuestions(survey, request.QuestionIds, currentUserId);

        var existingUserIds = survey.Assignments.Select(a => a.UserId).ToHashSet();
        var newUserIds = request.AssignedUserIds.ToHashSet();

        var toRemove = survey.Assignments.Where(a => !newUserIds.Contains(a.UserId)).ToList();
        _surveyRepository.RemoveAssignments(toRemove);

        var toAddUserIds = request.AssignedUserIds.Where(uid => !existingUserIds.Contains(uid)).ToList();
        var existingResponses = await _responseRepository.GetBySurveyIdAsync(survey.Id);
        var lastAnsweredAtByUserId = existingResponses
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Max(r => r.AnsweredAt));
        await AttachAssignments(survey, toAddUserIds, lastAnsweredAtByUserId);

        await _surveyRepository.SaveChangesAsync();

        return await GetByIdAsync(survey.Id, currentUserId, isAdmin);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey is null || !IsVisibleTo(survey, currentUserId, isAdmin))
            throw new KeyNotFoundException("Anket bulunamadı.");

        if (!CanModify(survey, currentUserId, isAdmin))
            throw new ForbiddenAccessException("Bu anketi silme yetkiniz yok.");

        _surveyRepository.Remove(survey);
        await _surveyRepository.SaveChangesAsync();
    }

    private async Task AttachQuestions(Survey survey, List<Guid> questionIds, Guid currentUserId)
    {
        for (int i = 0; i < questionIds.Count; i++)
        {
            var question = await _questionRepository.GetByIdAsync(questionIds[i]);
            if (question is null || !(question.OwnerId is null || question.OwnerId == currentUserId))
                throw new KeyNotFoundException($"Soru bulunamadı: {questionIds[i]}");

            var surveyQuestion = new SurveyQuestion
            {
                Id = Guid.NewGuid(),
                SurveyId = survey.Id,
                QuestionId = question.Id,
                Order = i + 1
            };
            _surveyRepository.AddSurveyQuestion(surveyQuestion);
        }
    }

    private async Task AttachAssignments(Survey survey, List<Guid> userIds, IReadOnlyDictionary<Guid, DateTime> lastAnsweredAtByUserId)
    {
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new KeyNotFoundException($"Kullanıcı bulunamadı: {userId}");

            var hasExistingResponses = lastAnsweredAtByUserId.TryGetValue(userId, out var lastAnsweredAt);

            var assignment = new SurveyAssignment
            {
                Id = Guid.NewGuid(),
                SurveyId = survey.Id,
                UserId = user.Id,
                IsCompleted = hasExistingResponses,
                CompletedAt = hasExistingResponses ? lastAnsweredAt : null
            };
            _surveyRepository.AddAssignment(assignment);
        }
    }

    public async Task<SurveyReportDto> GetReportAsync(Guid surveyId, Guid currentUserId, bool isAdmin)
    {
        var survey = await _surveyRepository.GetByIdWithResponsesAsync(surveyId);
        if (survey is null || !IsVisibleTo(survey, currentUserId, isAdmin))
            throw new KeyNotFoundException("Anket bulunamadı.");

        var responses = await _responseRepository.GetBySurveyIdAsync(surveyId);

        var completed = survey.Assignments.Where(a => a.IsCompleted).ToList();
        var pending = survey.Assignments.Where(a => !a.IsCompleted).ToList();

        var currentQuestionIds = survey.SurveyQuestions.Select(sq => sq.QuestionId).ToHashSet();

        var questionSummaries = survey.SurveyQuestions
            .OrderBy(sq => sq.Order)
            .Select(sq => new QuestionResponseSummaryDto
            {
                QuestionId = sq.QuestionId,
                QuestionText = sq.Question.Text,
                IsRemovedFromSurvey = false,
                UserAnswers = responses
                    .Where(r => r.QuestionId == sq.QuestionId)
                    .Select(r => new UserAnswerDto
                    {
                        UserEmail = r.User.Email,
                        SelectedOptionText = r.SelectedOption.Text
                    }).ToList()
            }).ToList();

        var removedQuestionSummaries = responses
            .Where(r => !currentQuestionIds.Contains(r.QuestionId))
            .GroupBy(r => r.QuestionId)
            .Select(g => new QuestionResponseSummaryDto
            {
                QuestionId = g.Key,
                QuestionText = g.First().Question.Text,
                IsRemovedFromSurvey = true,
                UserAnswers = g.Select(r => new UserAnswerDto
                {
                    UserEmail = r.User.Email,
                    SelectedOptionText = r.SelectedOption.Text
                }).ToList()
            });

        questionSummaries.AddRange(removedQuestionSummaries);

        return new SurveyReportDto
        {
            SurveyId = survey.Id,
            Title = survey.Title,
            TotalAssigned = survey.Assignments.Count,
            TotalCompleted = completed.Count,
            CompletedByUsers = completed.Select(a => new UserCompletionDto
            {
                UserId = a.UserId,
                Email = a.User.Email,
                CompletedAt = a.CompletedAt
            }).ToList(),
            PendingUsers = pending.Select(a => new UserCompletionDto
            {
                UserId = a.UserId,
                Email = a.User.Email,
                CompletedAt = null
            }).ToList(),
            QuestionSummaries = questionSummaries
        };
    }

    private static bool IsVisibleTo(Survey survey, Guid userId, bool isAdmin) =>
        isAdmin || survey.OwnerId == userId;

    private static bool CanModify(Survey survey, Guid userId, bool isAdmin) =>
        isAdmin || survey.OwnerId == userId;

    private static SurveyDto MapToDto(Survey survey)
    {
        return new SurveyDto
        {
            Id = survey.Id,
            Title = survey.Title,
            Description = survey.Description,
            StartDate = survey.StartDate,
            EndDate = survey.EndDate,
            IsActive = survey.IsActive,
            Questions = survey.SurveyQuestions
                .OrderBy(sq => sq.Order)
                .Select(sq => new SurveyQuestionDto
                {
                    QuestionId = sq.QuestionId,
                    QuestionText = sq.Question.Text,
                    Order = sq.Order
                }).ToList(),
            AssignedUsers = survey.Assignments
                .Select(a => new SurveyAssignedUserDto
                {
                    UserId = a.UserId,
                    Email = a.User.Email,
                    IsCompleted = a.IsCompleted
                }).ToList()
        };
    }
}
