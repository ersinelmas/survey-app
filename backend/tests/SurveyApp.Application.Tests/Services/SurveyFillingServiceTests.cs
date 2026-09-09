using Moq;
using SurveyApp.Application.DTOs.SurveyFilling;
using SurveyApp.Application.Services;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using Xunit;

namespace SurveyApp.Application.Tests.Services;

public class SurveyFillingServiceTests
{
    private readonly Mock<ISurveyAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<ISurveyResponseRepository> _responseRepository = new();
    private readonly SurveyFillingService _sut;

    public SurveyFillingServiceTests()
    {
        _sut = new SurveyFillingService(_assignmentRepository.Object, _responseRepository.Object);
    }

    private static Survey CreateActiveSurvey(string title = "Memnuniyet Anketi")
    {
        return new Survey
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Açıklama",
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
        };
    }

    private static SurveyAssignment CreateAssignment(Survey survey, Guid userId, bool isCompleted = false)
    {
        return new SurveyAssignment { Id = Guid.NewGuid(), SurveyId = survey.Id, Survey = survey, UserId = userId, IsCompleted = isCompleted };
    }

    [Fact]
    public async Task GetMyActiveSurveysAsync_ExcludesCompletedInactiveAndOutOfDateRangeSurveys()
    {
        var userId = Guid.NewGuid();
        var activeSurvey = CreateActiveSurvey("Aktif Anket");
        var completedAssignment = CreateAssignment(activeSurvey, userId, isCompleted: true);

        var inactiveSurvey = CreateActiveSurvey("Pasif Anket");
        inactiveSurvey.IsActive = false;
        var inactiveAssignment = CreateAssignment(inactiveSurvey, userId);

        var futureSurvey = CreateActiveSurvey("Gelecek Anket");
        futureSurvey.StartDate = DateTime.UtcNow.AddDays(5);
        futureSurvey.EndDate = DateTime.UtcNow.AddDays(10);
        var futureAssignment = CreateAssignment(futureSurvey, userId);

        var eligibleSurvey = CreateActiveSurvey("Doldurulabilir Anket");
        var eligibleAssignment = CreateAssignment(eligibleSurvey, userId);

        _assignmentRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new List<SurveyAssignment>
        {
            completedAssignment, inactiveAssignment, futureAssignment, eligibleAssignment,
        });

        var result = await _sut.GetMyActiveSurveysAsync(userId);

        Assert.Single(result);
        Assert.Equal("Doldurulabilir Anket", result[0].Title);
    }

    [Fact]
    public async Task GetSurveyToFillAsync_WhenNotAssigned_ThrowsKeyNotFoundException()
    {
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((SurveyAssignment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetSurveyToFillAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetSurveyToFillAsync_WhenAlreadyCompleted_ThrowsInvalidOperationException()
    {
        var survey = CreateActiveSurvey();
        var assignment = CreateAssignment(survey, Guid.NewGuid(), isCompleted: true);
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetSurveyToFillAsync(assignment.UserId, survey.Id));
    }

    [Fact]
    public async Task GetSurveyToFillAsync_WhenOutsideDateRange_ThrowsInvalidOperationException()
    {
        var survey = CreateActiveSurvey();
        survey.StartDate = DateTime.UtcNow.AddDays(2);
        survey.EndDate = DateTime.UtcNow.AddDays(5);
        var assignment = CreateAssignment(survey, Guid.NewGuid());
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetSurveyToFillAsync(assignment.UserId, survey.Id));
    }

    [Fact]
    public async Task GetSurveyToFillAsync_WhenEligible_ReturnsQuestionsAndOptionsOrdered()
    {
        var survey = CreateActiveSurvey();
        var template = new AnswerTemplate
        {
            Id = Guid.NewGuid(),
            Options = new List<AnswerOption>
            {
                new() { Id = Guid.NewGuid(), Text = "Hayır", Order = 2 },
                new() { Id = Guid.NewGuid(), Text = "Evet", Order = 1 },
            }
        };
        var question = new Question { Id = Guid.NewGuid(), Text = "Memnun musunuz?", AnswerTemplate = template };
        survey.SurveyQuestions = new List<SurveyQuestion>
        {
            new() { SurveyId = survey.Id, QuestionId = question.Id, Question = question, Order = 1 },
        };
        var assignment = CreateAssignment(survey, Guid.NewGuid());
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        var result = await _sut.GetSurveyToFillAsync(assignment.UserId, survey.Id);

        Assert.Single(result.Questions);
        Assert.Equal(2, result.Questions[0].Options.Count);
        Assert.Equal("Evet", result.Questions[0].Options[0].Text);
        Assert.Equal("Hayır", result.Questions[0].Options[1].Text);
    }

    [Fact]
    public async Task SubmitAsync_WhenNotAssigned_ThrowsKeyNotFoundException()
    {
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((SurveyAssignment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.SubmitAsync(Guid.NewGuid(), Guid.NewGuid(), new SubmitSurveyRequest()));
    }

    [Fact]
    public async Task SubmitAsync_WhenAlreadyCompleted_ThrowsInvalidOperationException()
    {
        var survey = CreateActiveSurvey();
        var assignment = CreateAssignment(survey, Guid.NewGuid(), isCompleted: true);
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitAsync(assignment.UserId, survey.Id, new SubmitSurveyRequest()));
    }

    [Fact]
    public async Task SubmitAsync_WhenMissingAnswerForAQuestion_ThrowsArgumentException()
    {
        var survey = CreateActiveSurvey();
        var question = new Question { Id = Guid.NewGuid(), Text = "Soru" };
        survey.SurveyQuestions = new List<SurveyQuestion> { new() { SurveyId = survey.Id, QuestionId = question.Id, Question = question } };
        var assignment = CreateAssignment(survey, Guid.NewGuid());
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SubmitAsync(assignment.UserId, survey.Id, new SubmitSurveyRequest { Answers = new List<SubmitAnswerDto>() }));
    }

    [Fact]
    public async Task SubmitAsync_WhenAnswerReferencesQuestionNotInSurvey_ThrowsArgumentException()
    {
        var survey = CreateActiveSurvey();
        var question = new Question { Id = Guid.NewGuid(), Text = "Soru" };
        survey.SurveyQuestions = new List<SurveyQuestion> { new() { SurveyId = survey.Id, QuestionId = question.Id, Question = question } };
        var assignment = CreateAssignment(survey, Guid.NewGuid());
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        var request = new SubmitSurveyRequest
        {
            Answers = new List<SubmitAnswerDto> { new() { QuestionId = Guid.NewGuid(), SelectedOptionId = Guid.NewGuid() } }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SubmitAsync(assignment.UserId, survey.Id, request));
    }

    [Fact]
    public async Task SubmitAsync_WhenValid_AddsResponsesMarksCompletedAndSaves()
    {
        var survey = CreateActiveSurvey();
        var question = new Question { Id = Guid.NewGuid(), Text = "Soru" };
        survey.SurveyQuestions = new List<SurveyQuestion> { new() { SurveyId = survey.Id, QuestionId = question.Id, Question = question } };
        var assignment = CreateAssignment(survey, Guid.NewGuid());
        _assignmentRepository.Setup(r => r.GetByUserAndSurveyAsync(assignment.UserId, survey.Id)).ReturnsAsync(assignment);

        var selectedOptionId = Guid.NewGuid();
        var request = new SubmitSurveyRequest
        {
            Answers = new List<SubmitAnswerDto> { new() { QuestionId = question.Id, SelectedOptionId = selectedOptionId } }
        };

        await _sut.SubmitAsync(assignment.UserId, survey.Id, request);

        _responseRepository.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<SurveyResponse>>(
            responses => responses.Single().QuestionId == question.Id && responses.Single().SelectedOptionId == selectedOptionId)), Times.Once);
        _responseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.True(assignment.IsCompleted);
        Assert.NotNull(assignment.CompletedAt);
    }
}
