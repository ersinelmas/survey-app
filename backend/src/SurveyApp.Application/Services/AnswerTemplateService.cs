using SurveyApp.Application.DTOs.AnswerTemplates;
using SurveyApp.Application.DTOs.Common;
using SurveyApp.Application.Exceptions;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class AnswerTemplateService
{
    private readonly IAnswerTemplateRepository _repository;
    private readonly ISurveyResponseRepository _responseRepository;

    public AnswerTemplateService(IAnswerTemplateRepository repository, ISurveyResponseRepository responseRepository)
    {
        _repository = repository;
        _responseRepository = responseRepository;
    }

    public async Task<List<AnswerTemplateDto>> GetAllAsync(Guid currentUserId)
    {
        var templates = await _repository.GetAllForUserAsync(currentUserId);
        return templates.Select(t => MapToDto(t, currentUserId)).ToList();
    }

    public async Task<PagedResult<AnswerTemplateDto>> GetPagedAsync(int page, int pageSize, Guid currentUserId)
    {
        var (items, totalCount) = await _repository.GetPagedForUserAsync(currentUserId, page, pageSize);
        return new PagedResult<AnswerTemplateDto>
        {
            Items = items.Select(t => MapToDto(t, currentUserId)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AnswerTemplateDto> GetByIdAsync(Guid id, Guid currentUserId)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template is null || !IsVisibleTo(template, currentUserId))
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        return MapToDto(template, currentUserId);
    }

    public async Task<AnswerTemplateDto> CreateAsync(CreateAnswerTemplateRequest request, Guid currentUserId)
    {
        var template = new AnswerTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerId = currentUserId,
            Options = request.Options.Select(o => new AnswerOption
            {
                Id = Guid.NewGuid(),
                Text = o.Text,
                Order = o.Order
            }).ToList()
        };

        await _repository.AddAsync(template);
        await _repository.SaveChangesAsync();

        return MapToDto(template, currentUserId);
    }

    public async Task<AnswerTemplateDto> UpdateAsync(Guid id, UpdateAnswerTemplateRequest request, Guid currentUserId, bool isAdmin)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template is null || !(isAdmin || IsVisibleTo(template, currentUserId)))
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        if (!CanModify(template, currentUserId, isAdmin))
            throw new ForbiddenAccessException("Bu cevap şablonunu düzenleme yetkiniz yok.");

        template.Name = request.Name;

        var incomingIds = request.Options.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToHashSet();
        var toRemove = template.Options.Where(o => !incomingIds.Contains(o.Id)).ToList();

        foreach (var option in toRemove)
        {
            var isUsed = await _responseRepository.IsOptionUsedInAnyResponseAsync(option.Id);
            if (isUsed)
                throw new InvalidOperationException($"'{option.Text}' şıkkı en az bir ankette cevaplanmış, silinemez. Metnini güncelleyebilir veya yeni bir şık ekleyebilirsiniz.");
        }

        foreach (var optionRequest in request.Options)
        {
            if (!optionRequest.Id.HasValue)
                continue;

            var existing = template.Options.FirstOrDefault(o => o.Id == optionRequest.Id.Value);
            if (existing is not null && existing.Text != optionRequest.Text)
            {
                var isUsed = await _responseRepository.IsOptionUsedInAnyResponseAsync(existing.Id);
                if (isUsed)
                    throw new InvalidOperationException($"'{existing.Text}' şıkkı en az bir ankette cevaplanmış, metni değiştirilemez. Bunun yerine yeni bir şık ekleyebilirsiniz.");
            }
        }

        foreach (var option in toRemove)
            template.Options.Remove(option);

        foreach (var optionRequest in request.Options)
        {
            if (optionRequest.Id.HasValue)
            {
                var existing = template.Options.FirstOrDefault(o => o.Id == optionRequest.Id.Value);
                if (existing is not null)
                {
                    existing.Text = optionRequest.Text;
                    existing.Order = optionRequest.Order;
                }
            }
            else
            {
                var newOption = new AnswerOption
                {
                    Id = Guid.NewGuid(),
                    AnswerTemplateId = template.Id,
                    Text = optionRequest.Text,
                    Order = optionRequest.Order
                };
                _repository.AddOption(newOption);
            }
        }

        await _repository.SaveChangesAsync();

        return MapToDto(template, currentUserId);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template is null || !(isAdmin || IsVisibleTo(template, currentUserId)))
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        if (!CanModify(template, currentUserId, isAdmin))
            throw new ForbiddenAccessException("Bu cevap şablonunu silme yetkiniz yok.");

        var isUsed = await _repository.IsUsedInAnyQuestionAsync(id);
        if (isUsed)
            throw new InvalidOperationException("Bu cevap şablonu bir veya daha fazla soruda kullanılıyor, silinemez. Önce ilgili soruları güncelleyin veya silin.");

        _repository.Remove(template);
        await _repository.SaveChangesAsync();
    }

    public async Task<AnswerTemplateDto> DuplicateAsync(Guid id, Guid currentUserId)
    {
        var source = await _repository.GetByIdAsync(id);
        if (source is null || !IsVisibleTo(source, currentUserId))
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        var copy = new AnswerTemplate
        {
            Id = Guid.NewGuid(),
            Name = source.Name,
            OwnerId = currentUserId,
            Options = source.Options
                .OrderBy(o => o.Order)
                .Select(o => new AnswerOption
                {
                    Id = Guid.NewGuid(),
                    Text = o.Text,
                    Order = o.Order
                }).ToList()
        };

        await _repository.AddAsync(copy);
        await _repository.SaveChangesAsync();

        return MapToDto(copy, currentUserId);
    }

    public async Task<AnswerTemplateDto> SetIsDefaultAsync(Guid id, bool isDefault, Guid currentUserId, bool isAdmin)
    {
        if (!isAdmin)
            throw new ForbiddenAccessException("Bu işlem için yetkiniz yok.");

        var template = await _repository.GetByIdAsync(id);
        if (template is null)
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        if (!(template.OwnerId is null || template.OwnerId == currentUserId))
            throw new ForbiddenAccessException("Başka bir kullanıcının içeriğini varsayılan yapamazsınız.");

        template.OwnerId = isDefault ? null : currentUserId;
        await _repository.SaveChangesAsync();

        return MapToDto(template, currentUserId);
    }

    private static bool IsVisibleTo(AnswerTemplate template, Guid userId) =>
        template.OwnerId is null || template.OwnerId == userId;

    private static bool CanModify(AnswerTemplate template, Guid userId, bool isAdmin) =>
        isAdmin || template.OwnerId == userId;

    private static AnswerTemplateDto MapToDto(AnswerTemplate template, Guid currentUserId)
    {
        return new AnswerTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            IsDefault = template.OwnerId is null,
            IsMine = template.OwnerId == currentUserId,
            Options = template.Options
                .OrderBy(o => o.Order)
                .Select(o => new AnswerOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    Order = o.Order
                }).ToList()
        };
    }
}
