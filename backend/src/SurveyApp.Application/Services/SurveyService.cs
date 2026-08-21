using SurveyApp.Application.DTOs.Surveys;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class SurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUserRepository _userRepository;

    public SurveyService(
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        IUserRepository userRepository)
    {
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _userRepository = userRepository;
    }

    public async Task<List<SurveyDto>> GetAllAsync()
    {
        var surveys = await _surveyRepository.GetAllAsync();
        return surveys.Select(MapToDto).ToList();
    }

    public async Task<SurveyDto> GetByIdAsync(Guid id)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey is null)
            throw new KeyNotFoundException("Anket bulunamadı.");

        return MapToDto(survey);
    }

    public async Task<SurveyDto> CreateAsync(CreateSurveyRequest request)
    {
        ValidateDates(request.StartDate, request.EndDate);

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        await AttachQuestions(survey, request.QuestionIds);
        await AttachAssignments(survey, request.AssignedUserIds);

        await _surveyRepository.AddAsync(survey);
        await _surveyRepository.SaveChangesAsync();

        return await GetByIdAsync(survey.Id); // ilişkileri include edilmiş haliyle geri döndürmek için tekrar çekiyoruz
    }

    public async Task<SurveyDto> UpdateAsync(Guid id, UpdateSurveyRequest request)
    {
        ValidateDates(request.StartDate, request.EndDate);

        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey is null)
            throw new KeyNotFoundException("Anket bulunamadı.");

        survey.Title = request.Title;
        survey.Description = request.Description;
        survey.StartDate = request.StartDate;
        survey.EndDate = request.EndDate;
        survey.IsActive = request.IsActive;

        survey.SurveyQuestions.Clear();
        survey.Assignments.Clear();

        await AttachQuestions(survey, request.QuestionIds);
        await AttachAssignments(survey, request.AssignedUserIds);

        await _surveyRepository.SaveChangesAsync();

        return await GetByIdAsync(survey.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var survey = await _surveyRepository.GetByIdAsync(id);
        if (survey is null)
            throw new KeyNotFoundException("Anket bulunamadı.");

        _surveyRepository.Remove(survey);
        await _surveyRepository.SaveChangesAsync();
    }

    private async Task AttachQuestions(Survey survey, List<Guid> questionIds)
    {
        for (int i = 0; i < questionIds.Count; i++)
        {
            var question = await _questionRepository.GetByIdAsync(questionIds[i]);
            if (question is null)
                throw new KeyNotFoundException($"Soru bulunamadı: {questionIds[i]}");

            survey.SurveyQuestions.Add(new SurveyQuestion
            {
                Id = Guid.NewGuid(),
                SurveyId = survey.Id,
                QuestionId = question.Id,
                Order = i + 1
            });
        }
    }

    private async Task AttachAssignments(Survey survey, List<Guid> userIds)
    {
        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new KeyNotFoundException($"Kullanıcı bulunamadı: {userId}");

            survey.Assignments.Add(new SurveyAssignment
            {
                Id = Guid.NewGuid(),
                SurveyId = survey.Id,
                UserId = user.Id,
                IsCompleted = false
            });
        }
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
    }

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