using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private const int MinSearchQueryLength = 3;
    private const int MaxSearchResults = 10;

    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        if (page is null && pageSize is null)
        {
            var users = await _userRepository.GetAllAsync();
            var result = users.Select(u => new { u.Id, u.Email, u.IsAdmin });
            return Ok(result);
        }

        var (items, totalCount) = await _userRepository.GetPagedAsync(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100));
        return Ok(new
        {
            Items = items.Select(u => new { u.Id, u.Email, u.IsAdmin }),
            TotalCount = totalCount,
            Page = page ?? 1,
            PageSize = pageSize ?? 20
        });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < MinSearchQueryLength)
            return Ok(Array.Empty<object>());

        var users = await _userRepository.SearchByEmailAsync(query.Trim(), MaxSearchResults);
        var result = users.Select(u => new { u.Id, u.Email });
        return Ok(result);
    }
}
