using Moq;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Application.Exceptions;
using SurveyApp.Application.Services;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using Xunit;

namespace SurveyApp.Application.Tests.Services;

public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _questionRepository = new();
    private readonly Mock<IAnswerTemplateRepository> _answerTemplateRepository = new();
    private readonly Mock<ISurveyResponseRepository> _responseRepository = new();
    private readonly QuestionService _sut;
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public QuestionServiceTests()
    {
        _sut = new QuestionService(_questionRepository.Object, _answerTemplateRepository.Object, _responseRepository.Object);
        _responseRepository.Setup(r => r.IsQuestionUsedInAnyResponseAsync(It.IsAny<Guid>())).ReturnsAsync(false);
    }

    private static AnswerTemplate CreateTemplate(Guid? ownerId, string name = "Memnuniyet") =>
        new() { Id = Guid.NewGuid(), Name = name, OwnerId = ownerId };

    private static Question CreateQuestion(AnswerTemplate template, Guid? ownerId, string text = "Hizmetten memnun kaldınız mı?") =>
        new() { Id = Guid.NewGuid(), Text = text, AnswerTemplateId = template.Id, AnswerTemplate = template, OwnerId = ownerId };

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var template = CreateTemplate(_ownerId);
        var question = CreateQuestion(template, _ownerId);
        _questionRepository.Setup(r => r.GetAllForUserAsync(_ownerId)).ReturnsAsync(new List<Question> { question });

        var result = await _sut.GetAllAsync(_ownerId);

        Assert.Single(result);
        Assert.Equal(question.Text, result[0].Text);
        Assert.Equal(template.Name, result[0].AnswerTemplateName);
        Assert.True(result[0].IsMine);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByCaller_ReturnsDto()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        var result = await _sut.GetByIdAsync(question.Id, _ownerId);

        Assert.Equal(question.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ThrowsKeyNotFoundException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(question.Id, _otherUserId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid(), _ownerId));
    }

    [Fact]
    public async Task CreateAsync_WhenTemplateNotFound_ThrowsKeyNotFoundException()
    {
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateAsync(new CreateQuestionRequest { Text = "Soru", AnswerTemplateId = Guid.NewGuid() }, _ownerId));

        _questionRepository.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenTemplateOwnedByAnotherUser_ThrowsKeyNotFoundException()
    {
        var template = CreateTemplate(_otherUserId);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateAsync(new CreateQuestionRequest { Text = "Soru", AnswerTemplateId = template.Id }, _ownerId));
    }

    [Fact]
    public async Task CreateAsync_WhenTemplateIsSystemDefault_AddsQuestionOwnedByCaller()
    {
        var template = CreateTemplate(null);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var result = await _sut.CreateAsync(new CreateQuestionRequest { Text = "Yeni Soru", AnswerTemplateId = template.Id }, _ownerId);

        Assert.Equal("Yeni Soru", result.Text);
        Assert.True(result.IsMine);
        _questionRepository.Verify(r => r.AddAsync(It.Is<Question>(q => q.Text == "Yeni Soru" && q.OwnerId == _ownerId)), Times.Once);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenQuestionNotFound_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateQuestionRequest { AnswerTemplateId = Guid.NewGuid() }, _ownerId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByAnotherUserAndCallerNotAdmin_ThrowsKeyNotFoundException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { AnswerTemplateId = Guid.NewGuid() }, _otherUserId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenSystemDefaultAndCallerNotAdmin_ThrowsForbiddenAccessException()
    {
        var question = CreateQuestion(CreateTemplate(null), null);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "x", AnswerTemplateId = question.AnswerTemplateId }, _otherUserId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenTemplateNotFound_ThrowsKeyNotFoundException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "x", AnswerTemplateId = Guid.NewGuid() }, _ownerId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesTextAndTemplateThenSaves()
    {
        var oldTemplate = CreateTemplate(_ownerId, "Eski Şablon");
        var newTemplate = CreateTemplate(_ownerId, "Yeni Şablon");
        var question = CreateQuestion(oldTemplate, _ownerId, "Eski Metin");
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(newTemplate.Id)).ReturnsAsync(newTemplate);

        var result = await _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "Yeni Metin", AnswerTemplateId = newTemplate.Id }, _ownerId, isAdmin: false);

        Assert.Equal("Yeni Metin", result.Text);
        Assert.Equal(newTemplate.Id, result.AnswerTemplateId);
        Assert.Equal(newTemplate.Name, result.AnswerTemplateName);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingTemplateOnAlreadyAnsweredQuestion_ThrowsInvalidOperationException()
    {
        var oldTemplate = CreateTemplate(_ownerId, "Eski Şablon");
        var newTemplate = CreateTemplate(_ownerId, "Yeni Şablon");
        var question = CreateQuestion(oldTemplate, _ownerId, "Metin");
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(newTemplate.Id)).ReturnsAsync(newTemplate);
        _responseRepository.Setup(r => r.IsQuestionUsedInAnyResponseAsync(question.Id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "Metin", AnswerTemplateId = newTemplate.Id }, _ownerId, isAdmin: false));

        Assert.Equal(oldTemplate.Id, question.AnswerTemplateId);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenTemplateUnchangedOnAlreadyAnsweredQuestion_UpdatesTextAndSaves()
    {
        var template = CreateTemplate(_ownerId, "Şablon");
        var question = CreateQuestion(template, _ownerId, "Eski Metin");
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);
        _responseRepository.Setup(r => r.IsQuestionUsedInAnyResponseAsync(question.Id)).ReturnsAsync(true);

        var result = await _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "Yeni Metin", AnswerTemplateId = template.Id }, _ownerId, isAdmin: false);

        Assert.Equal("Yeni Metin", result.Text);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), _ownerId, isAdmin: false));
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByAnotherUserAndCallerNotAdmin_ThrowsKeyNotFoundException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(question.Id, _otherUserId, isAdmin: false));

        _questionRepository.Verify(r => r.Remove(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenUsedInSurvey_ThrowsInvalidOperationException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _questionRepository.Setup(r => r.IsUsedInAnySurveyAsync(question.Id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(question.Id, _ownerId, isAdmin: false));

        _questionRepository.Verify(r => r.Remove(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotUsed_RemovesAndSaves()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _questionRepository.Setup(r => r.IsUsedInAnySurveyAsync(question.Id)).ReturnsAsync(false);

        await _sut.DeleteAsync(question.Id, _ownerId, isAdmin: false);

        _questionRepository.Verify(r => r.Remove(question), Times.Once);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DuplicateAsync_WhenSystemDefault_CreatesOwnedCopy()
    {
        var question = CreateQuestion(CreateTemplate(null), null, "Varsayılan Soru");
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        var result = await _sut.DuplicateAsync(question.Id, _otherUserId);

        Assert.NotEqual(question.Id, result.Id);
        Assert.Equal("Varsayılan Soru", result.Text);
        Assert.True(result.IsMine);
        _questionRepository.Verify(r => r.AddAsync(It.Is<Question>(q => q.OwnerId == _otherUserId && q.Id != question.Id)), Times.Once);
    }

    [Fact]
    public async Task DuplicateAsync_WhenNotVisibleToCaller_ThrowsKeyNotFoundException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DuplicateAsync(question.Id, _otherUserId));
    }

    [Fact]
    public async Task SetIsDefaultAsync_WhenCallerNotAdmin_ThrowsForbiddenAccessException()
    {
        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _sut.SetIsDefaultAsync(Guid.NewGuid(), true, _ownerId, isAdmin: false));
    }

    [Fact]
    public async Task SetIsDefaultAsync_WhenPublishingOwnQuestion_SetsOwnerIdToNull()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        var result = await _sut.SetIsDefaultAsync(question.Id, true, _ownerId, isAdmin: true);

        Assert.True(result.IsDefault);
        Assert.Null(question.OwnerId);
    }

    [Fact]
    public async Task SetIsDefaultAsync_WhenQuestionOwnedByAnotherUser_ThrowsForbiddenAccessException()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _otherUserId);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => _sut.SetIsDefaultAsync(question.Id, true, _ownerId, isAdmin: true));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedPagedResult()
    {
        var question = CreateQuestion(CreateTemplate(_ownerId), _ownerId);
        _questionRepository.Setup(r => r.GetPagedForUserAsync(_ownerId, 1, 10)).ReturnsAsync((new List<Question> { question }, 5));

        var result = await _sut.GetPagedAsync(1, 10, _ownerId);

        Assert.Single(result.Items);
        Assert.Equal(5, result.TotalCount);
    }
}
