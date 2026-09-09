using Microsoft.EntityFrameworkCore;
using SurveyApp.Core.Entities;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Data;

namespace SurveyApp.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(SurveyDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(List<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? role)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            query = Enum.TryParse<UserRole>(role, out var parsedRole)
                ? query.Where(u => u.Role == parsedRole)
                : query.Where(u => false);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}