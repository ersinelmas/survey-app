using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}