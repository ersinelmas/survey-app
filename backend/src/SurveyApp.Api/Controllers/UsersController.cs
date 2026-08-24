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
    public async Task<IActionResult> GetAll([FromQuery] string? role)
    {
        var users = await _userRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(role))
        {
            users = users.Where(u => u.Role.ToString() == role).ToList();
        }

        var result = users.Select(u => new { u.Id, u.Email, Role = u.Role.ToString() });
        return Ok(result);
    }
}