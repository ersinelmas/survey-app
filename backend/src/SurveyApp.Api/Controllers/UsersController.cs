using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? role, [FromQuery] int? page, [FromQuery] int? pageSize)
    {
        if (page is null && pageSize is null)
        {
            var users = await _userRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role.ToString() == role).ToList();
            }

            var result = users.Select(u => new { u.Id, u.Email, Role = u.Role.ToString() });
            return Ok(result);
        }

        var (items, totalCount) = await _userRepository.GetPagedAsync(Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? 20, 1, 100), role);
        return Ok(new
        {
            Items = items.Select(u => new { u.Id, u.Email, Role = u.Role.ToString() }),
            TotalCount = totalCount,
            Page = page ?? 1,
            PageSize = pageSize ?? 20
        });
    }
}