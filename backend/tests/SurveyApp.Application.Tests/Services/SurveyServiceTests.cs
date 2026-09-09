using Moq;
using SurveyApp.Application.DTOs.Surveys;
using SurveyApp.Application.Services;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using Xunit;

namespace SurveyApp.Application.Tests.Services;

public class SurveyServiceTests
{
    private readonly Mock<ISurveyRepository> _surveyRepository = new();
    private readonly Mock<IQuestionRepository> _questionRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ISurveyResponseRepository> _responseRepository = new();
    private readonly SurveyService _sut;

    public SurveyServiceTests()
    {
        _sut = new SurveyService(
            _surveyRepository.Object,
            _questionRepository.Object,
            _userRepository.Object,
            _responseRepository.Object);
    }

    private static Survey CreateSurvey(string title = "Memnuniyet Anketi")
    {
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Açıklama",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            IsActive = true,
        };
        return survey;
    }

    private void SetUpAddAsyncCapture()
    {
        Survey? capturedSurvey = null;
        _surveyRepository.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .Callback<Survey>(s => capturedSurvey = s)
            .Returns(Task.CompletedTask);
        _surveyRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => capturedSurvey);
    }

    [Fact]
    public async Task CreateAsync_WhenQuestionMissing_ThrowsKeyNotFoundException()
    {
        _questionRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Question?)null);

        var request = new CreateSurveyRequest { Title = "x", QuestionIds = new List<Guid> { Guid.NewGuid() } };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(request));

        _surveyRepository.Verify(r => r.AddAsync(It.IsAny<Survey>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenAssignedUserMissing_ThrowsKeyNotFoundException()
    {
        var question = new Question { Id = Guid.NewGuid(), Text = "Soru" };
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var request = new CreateSurveyRequest
        {
            Title = "x",
            QuestionIds = new List<Guid> { question.Id },
            AssignedUserIds = new List<Guid> { Guid.NewGuid() },
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(request));

        _surveyRepository.Verify(r => r.AddAsync(It.IsAny<Survey>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_AttachesQuestionsAndAssignmentsThenSaves()
    {
        SetUpAddAsyncCapture();
        var question = new Question { Id = Guid.NewGuid(), Text = "Soru" };
        var user = new User { Id = Guid.NewGuid(), Email = "user@user.com" };
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        var request = new CreateSurveyRequest
        {
            Title = "Yeni Anket",
            QuestionIds = new List<Guid> { question.Id },
            AssignedUserIds = new List<Guid> { user.Id },
        };

        var result = await _sut.CreateAsync(request);

        Assert.Equal("Yeni Anket", result.Title);
        _surveyRepository.Verify(r => r.AddSurveyQuestion(It.Is<SurveyQuestion>(sq => sq.QuestionId == question.Id && sq.Order == 1)), Times.Once);
        _surveyRepository.Verify(r => r.AddAssignment(It.Is<SurveyAssignment>(a => a.UserId == user.Id && !a.IsCompleted)), Times.Once);
        _surveyRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _surveyRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Survey?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(Guid.NewGuid(), new UpdateSurveyRequest()));
    }

    [Fact]
    public async Task UpdateAsync_PreservesCompletedAssignmentForUserStillInListAndRemovesDroppedOne()
    {
        var survey = CreateSurvey();
        var keptUser = new User { Id = Guid.NewGuid(), Email = "kept@user.com" };
        var droppedUser = new User { Id = Guid.NewGuid(), Email = "dropped@user.com" };
        var keptAssignment = new SurveyAssignment { Id = Guid.NewGuid(), SurveyId = survey.Id, UserId = keptUser.Id, User = keptUser, IsCompleted = true, CompletedAt = DateTime.UtcNow };
        var droppedAssignment = new SurveyAssignment { Id = Guid.NewGuid(), SurveyId = survey.Id, UserId = droppedUser.Id, User = droppedUser, IsCompleted = false };
        survey.Assignments = new List<SurveyAssignment> { keptAssignment, droppedAssignment };

        var question = new Question { Id = Guid.NewGuid(), Text = "Soru" };
        _surveyRepository.Setup(r => r.GetByIdAsync(survey.Id)).ReturnsAsync(survey);
        _questionRepository.Setup(r => r.GetByIdAsync(question.Id)).ReturnsAsync(question);

        var request = new UpdateSurveyRequest
        {
            Title = "Güncellenmiş",
            QuestionIds = new List<Guid> { question.Id },
            AssignedUserIds = new List<Guid> { keptUser.Id },
        };

        await _sut.UpdateAsync(survey.Id, request);

        _surveyRepository.Verify(r => r.RemoveAssignments(It.Is<IEnumerable<SurveyAssignment>>(
            list => list.Count() == 1 && list.Single().UserId == droppedUser.Id)), Times.Once);
        _surveyRepository.Verify(r => r.AddAssignment(It.IsAny<SurveyAssignment>()), Times.Never);
        Assert.True(keptAssignment.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _surveyRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Survey?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_RemovesAndSaves()
    {
        var survey = CreateSurvey();
        _surveyRepository.Setup(r => r.GetByIdAsync(survey.Id)).ReturnsAsync(survey);

        await _sut.DeleteAsync(survey.Id);

        _surveyRepository.Verify(r => r.Remove(survey), Times.Once);
        _surveyRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetReportAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _surveyRepository.Setup(r => r.GetByIdWithResponsesAsync(It.IsAny<Guid>())).ReturnsAsync((Survey?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetReportAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetReportAsync_SplitsCompletedAndPendingAndSummarizesAnswers()
    {
        var survey = CreateSurvey();
        var question = new Question { Id = Guid.NewGuid(), Text = "Memnun musunuz?" };
        var surveyQuestion = new SurveyQuestion { Id = Guid.NewGuid(), SurveyId = survey.Id, QuestionId = question.Id, Question = question, Order = 1 };
        survey.SurveyQuestions = new List<SurveyQuestion> { surveyQuestion };

        var completedUser = new User { Id = Guid.NewGuid(), Email = "completed@user.com" };
        var pendingUser = new User { Id = Guid.NewGuid(), Email = "pending@user.com" };
        survey.Assignments = new List<SurveyAssignment>
        {
            new() { UserId = completedUser.Id, User = completedUser, IsCompleted = true, CompletedAt = DateTime.UtcNow },
            new() { UserId = pendingUser.Id, User = pendingUser, IsCompleted = false },
        };

        var option = new AnswerOption { Id = Guid.NewGuid(), Text = "Evet" };
        var response = new SurveyResponse
        {
            SurveyId = survey.Id,
            UserId = completedUser.Id,
            User = completedUser,
            QuestionId = question.Id,
            SelectedOptionId = option.Id,
            SelectedOption = option,
        };

        _surveyRepository.Setup(r => r.GetByIdWithResponsesAsync(survey.Id)).ReturnsAsync(survey);
        _responseRepository.Setup(r => r.GetBySurveyIdAsync(survey.Id)).ReturnsAsync(new List<SurveyResponse> { response });

        var report = await _sut.GetReportAsync(survey.Id);

        Assert.Equal(2, report.TotalAssigned);
        Assert.Equal(1, report.TotalCompleted);
        Assert.Single(report.CompletedByUsers);
        Assert.Single(report.PendingUsers);
        Assert.Single(report.QuestionSummaries);
        Assert.Single(report.QuestionSummaries[0].UserAnswers);
        Assert.Equal("Evet", report.QuestionSummaries[0].UserAnswers[0].SelectedOptionText);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedPagedResult()
    {
        var survey = CreateSurvey();
        _surveyRepository.Setup(r => r.GetPagedAsync(1, 10)).ReturnsAsync((new List<Survey> { survey }, 3));

        var result = await _sut.GetPagedAsync(1, 10);

        Assert.Single(result.Items);
        Assert.Equal(3, result.TotalCount);
    }
}
