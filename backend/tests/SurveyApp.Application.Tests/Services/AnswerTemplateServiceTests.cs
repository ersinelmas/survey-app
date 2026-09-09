using Moq;
using SurveyApp.Application.DTOs.AnswerTemplates;
using SurveyApp.Application.Exceptions;
using SurveyApp.Application.Services;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using Xunit;

namespace SurveyApp.Application.Tests.Services;

public class AnswerTemplateServiceTests
{
    private readonly Mock<IAnswerTemplateRepository> _repository = new();
    private readonly Mock<ISurveyResponseRepository> _responseRepository = new();
    private readonly AnswerTemplateService _sut;
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public AnswerTemplateServiceTests()
    {
        _sut = new AnswerTemplateService(_repository.Object, _responseRepository.Object);
        _responseRepository.Setup(r => r.IsOptionUsedInAnyResponseAsync(It.IsAny<Guid>())).ReturnsAsync(false);
    }

    private static AnswerTemplate CreateTemplate(Guid? ownerId, string name = "Memnuniyet", params (Guid Id, string Text, int Order)[] options)
    {
        var template = new AnswerTemplate { Id = Guid.NewGuid(), Name = name, OwnerId = ownerId };
        template.Options = options
            .Select(o => new AnswerOption { Id = o.Id, Text = o.Text, Order = o.Order, AnswerTemplateId = template.Id })
            .ToList();
        return template;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTemplatesOrderedOptionsMappedToDto()
    {
        var optionId = Guid.NewGuid();
        var template = CreateTemplate(_ownerId, "Memnuniyet", (optionId, "Evet", 1));
        _repository.Setup(r => r.GetAllForUserAsync(_ownerId)).ReturnsAsync(new List<AnswerTemplate> { template });

        var result = await _sut.GetAllAsync(_ownerId);

        Assert.Single(result);
        Assert.Equal("Memnuniyet", result[0].Name);
        Assert.Equal("Evet", result[0].Options[0].Text);
        Assert.True(result[0].IsMine);
        Assert.False(result[0].IsDefault);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByCaller_ReturnsDto()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var result = await _sut.GetByIdAsync(template.Id, _ownerId);

        Assert.Equal(template.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSystemDefault_IsVisibleToAnyUserAndFlaggedAsDefault()
    {
        var template = CreateTemplate(null);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var result = await _sut.GetByIdAsync(template.Id, _otherUserId);

        Assert.True(result.IsDefault);
        Assert.False(result.IsMine);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ThrowsKeyNotFoundException()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(template.Id, _otherUserId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid(), _ownerId));
    }

    [Fact]
    public async Task CreateAsync_AddsTemplateOwnedByCallerAndSaves()
    {
        var request = new CreateAnswerTemplateRequest
        {
            Name = "Katılım Düzeyi",
            Options = new List<CreateAnswerOptionRequest>
            {
                new() { Text = "Katılıyorum", Order = 1 },
                new() { Text = "Katılmıyorum", Order = 2 },
            }
        };

        var result = await _sut.CreateAsync(request, _ownerId);

        Assert.Equal("Katılım Düzeyi", result.Name);
        Assert.Equal(2, result.Options.Count);
        Assert.True(result.IsMine);
        _repository.Verify(r => r.AddAsync(It.Is<AnswerTemplate>(t => t.Options.Count == 2 && t.OwnerId == _ownerId)), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateAnswerTemplateRequest(), _ownerId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnotherUserAndCallerNotAdmin_ThrowsKeyNotFoundException()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(template.Id, new UpdateAnswerTemplateRequest(), _otherUserId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenSystemDefaultAndCallerNotAdmin_ThrowsForbiddenAccessException()
    {
        var template = CreateTemplate(null);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _sut.UpdateAsync(template.Id, new UpdateAnswerTemplateRequest { Name = "x" }, _otherUserId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnotherUserButCallerIsAdmin_Succeeds()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var result = await _sut.UpdateAsync(
            template.Id,
            new UpdateAnswerTemplateRequest { Name = "Moderasyonla Güncellendi", Options = new List<UpdateAnswerOptionRequest>() },
            _otherUserId,
            isAdmin: true);

        Assert.Equal("Moderasyonla Güncellendi", result.Name);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RemovesMissingOptionsUpdatesExistingAndAddsNewOnes()
    {
        var keptOptionId = Guid.NewGuid();
        var removedOptionId = Guid.NewGuid();
        var template = CreateTemplate(_ownerId, "Eski Ad", (keptOptionId, "Eski Metin", 1), (removedOptionId, "Silinecek", 2));
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var request = new UpdateAnswerTemplateRequest
        {
            Name = "Yeni Ad",
            Options = new List<UpdateAnswerOptionRequest>
            {
                new() { Id = keptOptionId, Text = "Güncellenmiş Metin", Order = 1 },
                new() { Id = null, Text = "Yeni Şık", Order = 2 },
            }
        };

        var result = await _sut.UpdateAsync(template.Id, request, _ownerId, isAdmin: false);

        Assert.Equal("Yeni Ad", result.Name);
        Assert.DoesNotContain(template.Options, o => o.Id == removedOptionId);
        Assert.Contains(template.Options, o => o.Id == keptOptionId && o.Text == "Güncellenmiş Metin");
        _repository.Verify(r => r.AddOption(It.Is<AnswerOption>(o => o.Text == "Yeni Şık")), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenRemovingOptionAlreadyUsedInResponse_ThrowsInvalidOperationException()
    {
        var usedOptionId = Guid.NewGuid();
        var template = CreateTemplate(_ownerId, "Ad", (usedOptionId, "Cevaplanmış Şık", 1));
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);
        _responseRepository.Setup(r => r.IsOptionUsedInAnyResponseAsync(usedOptionId)).ReturnsAsync(true);

        var request = new UpdateAnswerTemplateRequest
        {
            Name = "Ad",
            Options = new List<UpdateAnswerOptionRequest>
            {
                new() { Id = null, Text = "Yeni Şık", Order = 1 },
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(template.Id, request, _ownerId, isAdmin: false));

        Assert.Contains(template.Options, o => o.Id == usedOptionId);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), _ownerId, isAdmin: false));
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByAnotherUserAndCallerNotAdmin_ThrowsKeyNotFoundException()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(template.Id, _otherUserId, isAdmin: false));

        _repository.Verify(r => r.Remove(It.IsAny<AnswerTemplate>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenUsedInQuestion_ThrowsInvalidOperationException()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);
        _repository.Setup(r => r.IsUsedInAnyQuestionAsync(template.Id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(template.Id, _ownerId, isAdmin: false));

        _repository.Verify(r => r.Remove(It.IsAny<AnswerTemplate>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotUsed_RemovesAndSaves()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);
        _repository.Setup(r => r.IsUsedInAnyQuestionAsync(template.Id)).ReturnsAsync(false);

        await _sut.DeleteAsync(template.Id, _ownerId, isAdmin: false);

        _repository.Verify(r => r.Remove(template), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DuplicateAsync_WhenSystemDefault_CreatesOwnedCopyWithSameOptions()
    {
        var optionId = Guid.NewGuid();
        var template = CreateTemplate(null, "Varsayılan Şablon", (optionId, "Evet", 1));
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var result = await _sut.DuplicateAsync(template.Id, _otherUserId);

        Assert.NotEqual(template.Id, result.Id);
        Assert.Equal("Varsayılan Şablon", result.Name);
        Assert.True(result.IsMine);
        Assert.Single(result.Options);
        _repository.Verify(r => r.AddAsync(It.Is<AnswerTemplate>(t => t.OwnerId == _otherUserId && t.Id != template.Id)), Times.Once);
    }

    [Fact]
    public async Task DuplicateAsync_WhenNotVisibleToCaller_ThrowsKeyNotFoundException()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DuplicateAsync(template.Id, _otherUserId));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedPagedResult()
    {
        var template = CreateTemplate(_ownerId);
        _repository.Setup(r => r.GetPagedForUserAsync(_ownerId, 2, 10)).ReturnsAsync((new List<AnswerTemplate> { template }, 25));

        var result = await _sut.GetPagedAsync(2, 10, _ownerId);

        Assert.Single(result.Items);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
    }
}
