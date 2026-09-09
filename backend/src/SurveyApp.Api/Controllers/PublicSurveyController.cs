using Microsoft.AspNetCore.Mvc;
using SurveyApp.Api.Extensions;
using SurveyApp.Application.DTOs.SurveyFilling;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/public/surveys")]
public class PublicSurveyController : ControllerBase
{
    private readonly SurveyFillingService _service;

    public PublicSurveyController(SurveyFillingService service)
    {
        _service = service;
    }

    [HttpGet("{surveyId}")]
    public async Task<IActionResult> GetPublicSurvey(Guid surveyId)
    {
        var survey = await _service.GetPublicSurveyAsync(surveyId, User.TryGetUserId());
        return Ok(survey);
    }

    [HttpPost("{surveyId}/submit")]
    public async Task<IActionResult> Submit(Guid surveyId, SubmitPublicSurveyRequest request)
    {
        await _service.SubmitPublicAsync(surveyId, User.TryGetUserId(), request);
        return Ok(new { message = "Anket başarıyla gönderildi." });
    }
}
