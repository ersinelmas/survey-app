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

    public new async Task<(List<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Users.AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<List<User>> SearchByEmailAsync(string query, int limit)
    {
        return await _context.Users
            .Where(u => EF.Functions.ILike(u.Email, $"%{query}%"))
            .OrderBy(u => u.Email)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> CountAdminsAsync()
    {
        return await _context.Users.CountAsync(u => u.IsAdmin);
    }
}