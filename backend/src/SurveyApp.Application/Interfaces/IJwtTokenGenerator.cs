using SurveyApp.Core.Entities;

namespace SurveyApp.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}