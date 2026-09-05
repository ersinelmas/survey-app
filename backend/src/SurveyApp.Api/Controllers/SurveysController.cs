using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Application.DTOs.Surveys;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SurveysController : ControllerBase
{
    private readonly SurveyService _service;

    public SurveysController(SurveyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var surveys = await _service.GetAllAsync();
        return Ok(surveys);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var survey = await _service.GetByIdAsync(id);
        return Ok(survey);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSurveyRequest request)
    {
        var survey = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = survey.Id }, survey);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateSurveyRequest request)
    {
        var survey = await _service.UpdateAsync(id, request);
        return Ok(survey);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/report")]
    public async Task<IActionResult> GetReport(Guid id)
    {
        var report = await _service.GetReportAsync(id);
        return Ok(report);
    }
}