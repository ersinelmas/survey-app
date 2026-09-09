using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Application.DTOs.AnswerTemplates;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AnswerTemplatesController : ControllerBase
{
    private readonly AnswerTemplateService _service;

    public AnswerTemplatesController(AnswerTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        if (page is null && pageSize is null)
        {
            var templates = await _service.GetAllAsync();
            return Ok(templates);
        }

        var result = await _service.GetPagedAsync(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var template = await _service.GetByIdAsync(id);
            return Ok(template);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAnswerTemplateRequest request)
    {
        var template = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateAnswerTemplateRequest request)
    {

        var template = await _service.UpdateAsync(id, request);
        return Ok(template);

    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}