namespace SurveyApp.Core.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    void Remove(T entity);
    Task SaveChangesAsync();
}