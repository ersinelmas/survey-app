using SurveyApp.Application.DTOs.Common;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Application.Exceptions;
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

    public async Task<List<QuestionDto>> GetAllAsync(Guid currentUserId)
    {
        var questions = await _questionRepository.GetAllForUserAsync(currentUserId);
        return questions.Select(q => MapToDto(q, currentUserId)).ToList();
    }

    public async Task<PagedResult<QuestionDto>> GetPagedAsync(int page, int pageSize, Guid currentUserId)
    {
        var (items, totalCount) = await _questionRepository.GetPagedForUserAsync(currentUserId, page, pageSize);
        return new PagedResult<QuestionDto>
        {
            Items = items.Select(q => MapToDto(q, currentUserId)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<QuestionDto> GetByIdAsync(Guid id, Guid currentUserId)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null || !IsVisibleTo(question, currentUserId))
            throw new KeyNotFoundException("Soru bulunamadı.");

        return MapToDto(question, currentUserId);
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionRequest request, Guid currentUserId)
    {
        var template = await _answerTemplateRepository.GetByIdAsync(request.AnswerTemplateId);
        if (template is null || !(template.OwnerId is null || template.OwnerId == currentUserId))
            throw new KeyNotFoundException("Belirtilen cevap şablonu bulunamadı.");

        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = request.Text,
            AnswerTemplateId = request.AnswerTemplateId,
            OwnerId = currentUserId
        };

        await _questionRepository.AddAsync(question);
        await _questionRepository.SaveChangesAsync();

        question.AnswerTemplate = template;
        return MapToDto(question, currentUserId);
    }

    public async Task<QuestionDto> UpdateAsync(Guid id, UpdateQuestionRequest request, Guid currentUserId, bool isAdmin)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null || !(isAdmin || IsVisibleTo(question, currentUserId)))
            throw new KeyNotFoundException("Soru bulunamadı.");

        if (!CanModify(question, currentUserId, isAdmin))
            throw new ForbiddenAccessException("Bu soruyu düzenleme yetkiniz yok.");

        AnswerTemplate template;
        if (question.AnswerTemplateId != request.AnswerTemplateId)
        {
            var newTemplate = await _answerTemplateRepository.GetByIdAsync(request.AnswerTemplateId);
            if (newTemplate is null || !(newTemplate.OwnerId is null || newTemplate.OwnerId == currentUserId))
                throw new KeyNotFoundException("Belirtilen cevap şablonu bulunamadı.");

            var isUsed = await _responseRepository.IsQuestionUsedInAnyResponseAsync(id);
            if (isUsed)
                throw new InvalidOperationException("Bu soru en az bir ankette cevaplanmış, cevap şablonu değiştirilemez.");

            template = newTemplate;
        }
        else
        {
            template = question.AnswerTemplate!;
        }

        question.Text = request.Text;
        question.AnswerTemplateId = request.AnswerTemplateId;
        question.AnswerTemplate = template;

        await _questionRepository.SaveChangesAsync();

        return MapToDto(question, currentUserId);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null || !(isAdmin || IsVisibleTo(question, currentUserId)))
            throw new KeyNotFoundException("Soru bulunamadı.");

        if (!CanModify(question, currentUserId, isAdmin))
            throw new ForbiddenAccessException("Bu soruyu silme yetkiniz yok.");

        var isUsed = await _questionRepository.IsUsedInAnySurveyAsync(id);
        if (isUsed)
            throw new InvalidOperationException("Bu soru bir veya daha fazla ankette kullanılıyor, silinemez. Önce ilgili anketlerden çıkarın.");

        _questionRepository.Remove(question);
        await _questionRepository.SaveChangesAsync();
    }

    public async Task<QuestionDto> DuplicateAsync(Guid id, Guid currentUserId)
    {
        var source = await _questionRepository.GetByIdAsync(id);
        if (source is null || !IsVisibleTo(source, currentUserId))
            throw new KeyNotFoundException("Soru bulunamadı.");

        var copy = new Question
        {
            Id = Guid.NewGuid(),
            Text = source.Text,
            AnswerTemplateId = source.AnswerTemplateId,
            OwnerId = currentUserId
        };

        await _questionRepository.AddAsync(copy);
        await _questionRepository.SaveChangesAsync();

        copy.AnswerTemplate = source.AnswerTemplate;
        return MapToDto(copy, currentUserId);
    }

    public async Task<QuestionDto> SetIsDefaultAsync(Guid id, bool isDefault, Guid currentUserId, bool isAdmin)
    {
        if (!isAdmin)
            throw new ForbiddenAccessException("Bu işlem için yetkiniz yok.");

        var question = await _questionRepository.GetByIdAsync(id);
        if (question is null)
            throw new KeyNotFoundException("Soru bulunamadı.");

        if (!(question.OwnerId is null || question.OwnerId == currentUserId))
            throw new ForbiddenAccessException("Başka bir kullanıcının içeriğini varsayılan yapamazsınız.");

        question.OwnerId = isDefault ? null : currentUserId;
        await _questionRepository.SaveChangesAsync();

        return MapToDto(question, currentUserId);
    }

    private static bool IsVisibleTo(Question question, Guid userId) =>
        question.OwnerId is null || question.OwnerId == userId;

    private static bool CanModify(Question question, Guid userId, bool isAdmin) =>
        isAdmin || question.OwnerId == userId;

    private static QuestionDto MapToDto(Question question, Guid currentUserId)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            AnswerTemplateId = question.AnswerTemplateId,
            AnswerTemplateName = question.AnswerTemplate.Name,
            IsDefault = question.OwnerId is null,
            IsMine = question.OwnerId == currentUserId
        };
    }
}
