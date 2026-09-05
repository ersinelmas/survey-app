using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}
