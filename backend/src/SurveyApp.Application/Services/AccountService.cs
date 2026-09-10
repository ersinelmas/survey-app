using SurveyApp.Application.Interfaces;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class AccountService
{
    private readonly IUserRepository _userRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerTemplateRepository _answerTemplateRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AccountService(
        IUserRepository userRepository,
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        IAnswerTemplateRepository answerTemplateRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _answerTemplateRepository = answerTemplateRepository;
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
}
