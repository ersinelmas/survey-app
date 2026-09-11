using SurveyApp.Application.DTOs.Account;
using SurveyApp.Application.Interfaces;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class AccountService
{
    private readonly IUserRepository _userRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerTemplateRepository _answerTemplateRepository;
    private readonly ISurveyResponseRepository _responseRepository;
    private readonly ISurveyAssignmentRepository _assignmentRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AccountService(
        IUserRepository userRepository,
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        IAnswerTemplateRepository answerTemplateRepository,
        ISurveyResponseRepository responseRepository,
        ISurveyAssignmentRepository assignmentRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _answerTemplateRepository = answerTemplateRepository;
        _responseRepository = responseRepository;
        _assignmentRepository = assignmentRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task DeleteOwnAccountAsync(Guid userId, string password)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Şifre hatalı.");

        if (user.IsAdmin)
        {
            var adminCount = await _userRepository.CountAdminsAsync();
            if (adminCount <= 1)
                throw new InvalidOperationException("Sistemdeki tek admin hesabı silinemez. Önce başka bir kullanıcıyı admin yapın.");
        }

        // Sahip olunan Survey/Question/AnswerTemplate'ler görünürlük kuralı gereği
        // sadece kendi içinde birbirine referans verir, bu yüzden sırayla silmek yeterli.
        var surveys = await _surveyRepository.GetAllForUserAsync(userId);
        foreach (var survey in surveys)
            _surveyRepository.Remove(survey);
        await _surveyRepository.SaveChangesAsync();

        var questions = (await _questionRepository.GetAllForUserAsync(userId))
            .Where(q => q.OwnerId == userId)
            .ToList();
        foreach (var question in questions)
            _questionRepository.Remove(question);
        await _questionRepository.SaveChangesAsync();

        var templates = (await _answerTemplateRepository.GetAllForUserAsync(userId))
            .Where(t => t.OwnerId == userId)
            .ToList();
        foreach (var template in templates)
            _answerTemplateRepository.Remove(template);
        await _answerTemplateRepository.SaveChangesAsync();

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<DataExportDto> GetDataExportAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var ownedSurveys = await _surveyRepository.GetAllForUserAsync(userId);
        var ownedQuestions = (await _questionRepository.GetAllForUserAsync(userId))
            .Where(q => q.OwnerId == userId)
            .ToList();
        var ownedTemplates = (await _answerTemplateRepository.GetAllForUserAsync(userId))
            .Where(t => t.OwnerId == userId)
            .ToList();
        var myResponses = await _responseRepository.GetByUserIdAsync(userId);
        var myAssignments = await _assignmentRepository.GetByUserIdAsync(userId);

        return new DataExportDto
        {
            Account = new UserExportDto
            {
                Id = user.Id,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                CreatedAt = user.CreatedAt,
            },
            OwnedSurveys = ownedSurveys.Select(s => new OwnedSurveyExportDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive,
                IsPublic = s.IsPublic,
                Questions = s.SurveyQuestions.OrderBy(sq => sq.Order).Select(sq => sq.Question.Text).ToList(),
                AssignedUserCount = s.Assignments.Count,
            }).ToList(),
            OwnedQuestions = ownedQuestions.Select(q => new OwnedQuestionExportDto
            {
                Id = q.Id,
                Text = q.Text,
                Type = q.Type,
            }).ToList(),
            OwnedAnswerTemplates = ownedTemplates.Select(t => new OwnedAnswerTemplateExportDto
            {
                Id = t.Id,
                Name = t.Name,
                Options = t.Options.OrderBy(o => o.Order).Select(o => o.Text).ToList(),
            }).ToList(),
            MyResponses = BuildMyResponses(myResponses),
            MyAssignments = myAssignments.Select(a => new MyAssignmentExportDto
            {
                SurveyTitle = a.Survey.Title,
                IsCompleted = a.IsCompleted,
                CompletedAt = a.CompletedAt,
            }).ToList(),
        };
    }

    private static List<MyResponseExportDto> BuildMyResponses(List<SurveyResponse> responses)
    {
        return responses
            .GroupBy(r => (r.SurveyId, r.QuestionId))
            .Select(g =>
            {
                var first = g.First();
                var optionTexts = g
                    .Where(r => r.SelectedOption is not null)
                    .OrderBy(r => r.SelectedOption!.Order)
                    .Select(r => r.SelectedOption!.Text);

                return new MyResponseExportDto
                {
                    SurveyTitle = first.Survey.Title,
                    QuestionText = first.Question.Text,
                    Answer = first.TextValue ?? string.Join(", ", optionTexts),
                    AnsweredAt = first.AnsweredAt,
                };
            })
            .OrderBy(r => r.AnsweredAt)
            .ToList();
    }
}
