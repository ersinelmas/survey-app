using SurveyApp.Core.Entities;

namespace SurveyApp.Core.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    new Task<(List<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<List<User>> SearchByEmailAsync(string query, int limit);
    Task<int> CountAdminsAsync();
}