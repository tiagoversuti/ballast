using Ballast.Application.DTOs;

namespace Ballast.Application.Interfaces;

public interface ITodoService
{
    Task<List<TodoDto>> GetAllAsync();
    Task<TodoDto?> GetByIdAsync(Guid id);
    Task<TodoDto> CreateAsync(CreateTodoDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateTodoDto dto);
    Task<bool> DeleteAsync(Guid id);
}
