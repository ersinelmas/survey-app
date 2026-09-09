using SurveyApp.Application.DTOs.Common;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class QuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerTemplateRepository _answerTemplateRepository;
    private readonly ISurveyResponseRepository _responseRepository;

    public QuestionService(
        IQuestionRepository questionRepository,
        IAnswerTemplateRepository answerTemplateRepository,
        ISurveyResponseRepository responseRepository)
    {
        _questionRepository = questionRepository;
        _answerTemplateRepository = answerTemplateRepository;
        _responseRepository = responseRepository;
    }

    public async Task<List<QuestionDto>> GetAllAsync()
    {
        var questions = await _questionRepository.GetAllAsync();
        return questions.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<QuestionDto>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _questionRepository.GetPagedAsync(page, pageSize);
        return new PagedResult<QuestionDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<QuestionDto> GetByIdAsync(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null)
            throw new KeyNotFoundException("Soru bulunamadı.");

        return MapToDto(question);
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionRequest request)
    {
        var template = await _answerTemplateRepository.GetByIdAsync(request.AnswerTemplateId);
        if (template is null)
            throw new KeyNotFoundException("Belirtilen cevap şablonu bulunamadı.");

        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = request.Text,
            AnswerTemplateId = request.AnswerTemplateId
        };

        await _questionRepository.AddAsync(question);
        await _questionRepository.SaveChangesAsync();

        question.AnswerTemplate = template;
        return MapToDto(question);
    }

    public async Task<QuestionDto> UpdateAsync(Guid id, UpdateQuestionRequest request)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null)
            throw new KeyNotFoundException("Soru bulunamadı.");

        var template = await _answerTemplateRepository.GetByIdAsync(request.AnswerTemplateId);
        if (template is null)
            throw new KeyNotFoundException("Belirtilen cevap şablonu bulunamadı.");

        if (question.AnswerTemplateId != request.AnswerTemplateId)
        {
            var isUsed = await _responseRepository.IsQuestionUsedInAnyResponseAsync(id);
            if (isUsed)
                throw new InvalidOperationException("Bu soru en az bir ankette cevaplanmış, cevap şablonu değiştirilemez.");
        }

        question.Text = request.Text;
        question.AnswerTemplateId = request.AnswerTemplateId;
        question.AnswerTemplate = template;

        await _questionRepository.SaveChangesAsync();

        return MapToDto(question);
    }

    public async Task DeleteAsync(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null)
            throw new KeyNotFoundException("Soru bulunamadı.");

        var isUsed = await _questionRepository.IsUsedInAnySurveyAsync(id);
        if (isUsed)
            throw new InvalidOperationException("Bu soru bir veya daha fazla ankette kullanılıyor, silinemez. Önce ilgili anketlerden çıkarın.");

        _questionRepository.Remove(question);
        await _questionRepository.SaveChangesAsync();
    }

    private static QuestionDto MapToDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            AnswerTemplateId = question.AnswerTemplateId,
            AnswerTemplateName = question.AnswerTemplate.Name
        };
    }
}