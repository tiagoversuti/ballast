using Ballast.Application.DTOs;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;

namespace Ballast.Application.Services;

public class TodoService(ITodoRepository repository) : ITodoService
{
    public async Task<List<TodoDto>> GetAllAsync(Guid userId)
    {
        var items = await repository.GetAllAsync(userId);
        return items.Select(ToDto).ToList();
    }

    public async Task<TodoDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null || item.UserId != userId) return null;
        return ToDto(item);
    }

    public async Task<TodoDto> CreateAsync(CreateTodoDto dto, Guid userId)
    {
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await repository.AddAsync(item);
        return ToDto(item);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTodoDto dto, Guid userId)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null || item.UserId != userId) return false;

        item.Title = dto.Title;
        item.IsDone = dto.IsDone;
        await repository.UpdateAsync(item);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null || item.UserId != userId) return false;

        await repository.DeleteAsync(id);
        return true;
    }

    private static TodoDto ToDto(TodoItem item) =>
        new(item.Id, item.Title, item.IsDone, item.CreatedAt);
}
