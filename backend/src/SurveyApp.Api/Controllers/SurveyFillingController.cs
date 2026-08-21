using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Application.DTOs.SurveyFilling;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/my-surveys")]
[Authorize]
public class SurveyFillingController : ControllerBase
{
    private readonly SurveyFillingService _service;

    public SurveyFillingController(SurveyFillingService service)
    {
        _service = service;
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(idClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyActiveSurveys()
    {
        var userId = GetCurrentUserId();
        var surveys = await _service.GetMyActiveSurveysAsync(userId);
        return Ok(surveys);
    }

    [HttpGet("{surveyId}")]
    public async Task<IActionResult> GetSurveyToFill(Guid surveyId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var survey = await _service.GetSurveyToFillAsync(userId, surveyId);
            return Ok(survey);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{surveyId}/submit")]
    public async Task<IActionResult> Submit(Guid surveyId, SubmitSurveyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _service.SubmitAsync(userId, surveyId, request);
            return Ok(new { message = "Anket başarıyla gönderildi." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}