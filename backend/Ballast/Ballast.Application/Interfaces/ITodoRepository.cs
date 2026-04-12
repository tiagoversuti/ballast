using Ballast.Application.Entities;

namespace Ballast.Application.Interfaces;

public interface ITodoRepository
{
    Task<List<TodoItem>> GetAllAsync(Guid userId);
    Task<TodoItem?> GetByIdAsync(Guid id);
    Task AddAsync(TodoItem item);
    Task UpdateAsync(TodoItem item);
    Task DeleteAsync(Guid id);
}
