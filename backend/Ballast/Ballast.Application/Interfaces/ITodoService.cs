using Ballast.Application.DTOs;

namespace Ballast.Application.Interfaces;

public interface ITodoService
{
    Task<List<TodoDto>> GetAllAsync(Guid userId);
    Task<TodoDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TodoDto> CreateAsync(CreateTodoDto dto, Guid userId);
    Task<bool> UpdateAsync(Guid id, UpdateTodoDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
