using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Api.Extensions;
using SurveyApp.Application.DTOs.Common;
using SurveyApp.Application.DTOs.Questions;
using SurveyApp.Application.Services;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
            var questions = await _service.GetAllAsync(User.GetUserId());
            return Ok(questions);
        }

        var result = await _service.GetPagedAsync(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100), User.GetUserId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var question = await _service.GetByIdAsync(id, User.GetUserId());
        return Ok(question);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionRequest request)
    {
        var question = await _service.CreateAsync(request, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateQuestionRequest request)
    {
        var question = await _service.UpdateAsync(id, request, User.GetUserId(), User.IsAdmin());
        return Ok(question);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id, User.GetUserId(), User.IsAdmin());
        return NoContent();
    }

    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var copy = await _service.DuplicateAsync(id, User.GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = copy.Id }, copy);
    }

    [HttpPut("{id}/default")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> SetIsDefault(Guid id, SetIsDefaultRequest request)
    {
        var question = await _service.SetIsDefaultAsync(id, request.IsDefault, User.GetUserId(), User.IsAdmin());
        return Ok(question);
    }
}
