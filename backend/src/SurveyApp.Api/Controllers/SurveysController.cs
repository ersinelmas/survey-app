using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Api.Extensions;
using SurveyApp.Application.DTOs.Surveys;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SurveysController : ControllerBase
{
    private readonly SurveyService _service;

    public SurveysController(SurveyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        if (page is null && pageSize is null)
        {
            var surveys = await _service.GetAllAsync(User.GetUserId());
            return Ok(surveys);
        }

        var result = await _service.GetPagedAsync(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100), User.GetUserId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var survey = await _service.GetByIdAsync(id, User.GetUserId(), User.IsAdmin());
        return Ok(survey);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSurveyRequest request)
    {
        var survey = await _service.CreateAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateSurveyRequest request)
    {
        var survey = await _service.UpdateAsync(id, request, User.GetUserId(), User.IsAdmin());
        return Ok(survey);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, User.GetUserId(), User.IsAdmin());
        return NoContent();
    }

    [HttpGet("{id}/report")]
    public async Task<IActionResult> GetReport(Guid id)
    {
        var report = await _service.GetReportAsync(id, User.GetUserId(), User.IsAdmin());
        return Ok(report);
    }
}
