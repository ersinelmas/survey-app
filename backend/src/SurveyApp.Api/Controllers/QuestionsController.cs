using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class QuestionsController : ControllerBase
{
    private readonly QuestionService _service;

    public QuestionsController(QuestionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        if (page is null && pageSize is null)
        {
            var questions = await _service.GetAllAsync();
            return Ok(questions);
        }

        var result = await _service.GetPagedAsync(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var question = await _service.GetByIdAsync(id);
        return Ok(question);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionRequest request)
    {
        var question = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateQuestionRequest request)
    {
        var question = await _service.UpdateAsync(id, request);
        return Ok(question);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}