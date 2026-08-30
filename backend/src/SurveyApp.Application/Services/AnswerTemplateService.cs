using SurveyApp.Application.DTOs.AnswerTemplates;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public class AnswerTemplateService
{
    private readonly IAnswerTemplateRepository _repository;

    public AnswerTemplateService(IAnswerTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AnswerTemplateDto>> GetAllAsync()
    {
        var templates = await _repository.GetAllAsync();
        return templates.Select(MapToDto).ToList();
    }

    public async Task<AnswerTemplateDto> GetByIdAsync(Guid id)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template is null)
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        return MapToDto(template);
    }

    public async Task<AnswerTemplateDto> CreateAsync(CreateAnswerTemplateRequest request)
    {
        var template = new AnswerTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Options = request.Options.Select(o => new AnswerOption
            {
                Id = Guid.NewGuid(),
                Text = o.Text,
                Order = o.Order
            }).ToList()
        };

        await _repository.AddAsync(template);
        await _repository.SaveChangesAsync();

        return MapToDto(template);
    }

    public async Task<AnswerTemplateDto> UpdateAsync(Guid id, UpdateAnswerTemplateRequest request)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template is null)
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        template.Name = request.Name;

        var incomingIds = request.Options.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToHashSet();
        var toRemove = template.Options.Where(o => !incomingIds.Contains(o.Id)).ToList();
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

        return MapToDto(template);
    }

    public async Task DeleteAsync(Guid id)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template is null)
            throw new KeyNotFoundException("Cevap şablonu bulunamadı.");

        var isUsed = await _repository.IsUsedInAnyQuestionAsync(id);
        if (isUsed)
            throw new InvalidOperationException("Bu cevap şablonu bir veya daha fazla soruda kullanılıyor, silinemez. Önce ilgili soruları güncelleyin veya silin.");

        _repository.Remove(template);
        await _repository.SaveChangesAsync();
    }

    private static AnswerTemplateDto MapToDto(AnswerTemplate template)
    {
        return new AnswerTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
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