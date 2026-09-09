using Moq;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Application.Services;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using Xunit;

namespace SurveyApp.Application.Tests.Services;

public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _questionRepository = new();
    private readonly Mock<IAnswerTemplateRepository> _answerTemplateRepository = new();
    private readonly QuestionService _sut;

    public QuestionServiceTests()
    {
        _sut = new QuestionService(_questionRepository.Object, _answerTemplateRepository.Object);
    }

    private static AnswerTemplate CreateTemplate(string name = "Memnuniyet") =>
        new() { Id = Guid.NewGuid(), Name = name };

    private static Question CreateQuestion(AnswerTemplate template, string text = "Hizmetten memnun kaldınız mı?") =>
        new() { Id = Guid.NewGuid(), Text = text, AnswerTemplateId = template.Id, AnswerTemplate = template };

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var template = CreateTemplate();
        var question = CreateQuestion(template);
        _questionRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Question> { question });

        var result = await _sut.GetAllAsync();

        Assert.Single(result);
        Assert.Equal(question.Text, result[0].Text);
        Assert.Equal(template.Name, result[0].AnswerTemplateName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsDto()
    {
        var question = CreateQuestion(CreateTemplate());
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        var result = await _sut.GetByIdAsync(question.Id);

        Assert.Equal(question.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_WhenTemplateNotFound_ThrowsKeyNotFoundException()
    {
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateAsync(new CreateQuestionRequest { Text = "Soru", AnswerTemplateId = Guid.NewGuid() }));

        _questionRepository.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenTemplateFound_AddsQuestionAndSaves()
    {
        var template = CreateTemplate();
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(template.Id)).ReturnsAsync(template);

        var result = await _sut.CreateAsync(new CreateQuestionRequest { Text = "Yeni Soru", AnswerTemplateId = template.Id });

        Assert.Equal("Yeni Soru", result.Text);
        Assert.Equal(template.Name, result.AnswerTemplateName);
        _questionRepository.Verify(r => r.AddAsync(It.Is<Question>(q => q.Text == "Yeni Soru")), Times.Once);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenQuestionNotFound_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateQuestionRequest { AnswerTemplateId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task UpdateAsync_WhenTemplateNotFound_ThrowsKeyNotFoundException()
    {
        var question = CreateQuestion(CreateTemplate());
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AnswerTemplate?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "x", AnswerTemplateId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_UpdatesTextAndTemplateThenSaves()
    {
        var oldTemplate = CreateTemplate("Eski Şablon");
        var newTemplate = CreateTemplate("Yeni Şablon");
        var question = CreateQuestion(oldTemplate, "Eski Metin");
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _answerTemplateRepository.Setup(r => r.GetByIdAsync(newTemplate.Id)).ReturnsAsync(newTemplate);

        var result = await _sut.UpdateAsync(question.Id, new UpdateQuestionRequest { Text = "Yeni Metin", AnswerTemplateId = newTemplate.Id });

        Assert.Equal("Yeni Metin", result.Text);
        Assert.Equal(newTemplate.Id, result.AnswerTemplateId);
        Assert.Equal(newTemplate.Name, result.AnswerTemplateName);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WhenUsedInSurvey_ThrowsInvalidOperationException()
    {
        var question = CreateQuestion(CreateTemplate());
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _questionRepository.Setup(r => r.IsUsedInAnySurveyAsync(question.Id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(question.Id));

        _questionRepository.Verify(r => r.Remove(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotUsed_RemovesAndSaves()
    {
        var question = CreateQuestion(CreateTemplate());
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _questionRepository.Setup(r => r.IsUsedInAnySurveyAsync(question.Id)).ReturnsAsync(false);

        await _sut.DeleteAsync(question.Id);

        _questionRepository.Verify(r => r.Remove(question), Times.Once);
        _questionRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedPagedResult()
    {
        var question = CreateQuestion(CreateTemplate());
        _questionRepository.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync((new List<Question> { question }, 5));

        var result = await _sut.GetPagedAsync(1, 10);

        Assert.Single(result.Items);
        Assert.Equal(5, result.TotalCount);
    }
}
