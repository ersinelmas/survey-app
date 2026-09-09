using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<(List<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
}