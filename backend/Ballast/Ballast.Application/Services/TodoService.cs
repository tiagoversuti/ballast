using Ballast.Application.DTOs;
using Ballast.Application.Entities;
using Ballast.Application.Interfaces;

namespace Ballast.Application.Services;

public class TodoService(ITodoRepository repository) : ITodoService
{
    public async Task<List<TodoDto>> GetAllAsync()
    {
        var items = await repository.GetAllAsync();
        return items.Select(ToDto).ToList();
    }

    public async Task<TodoDto?> GetByIdAsync(Guid id)
    {
        var item = await repository.GetByIdAsync(id);
        return item is null ? null : ToDto(item);
    }

    public async Task<TodoDto> CreateAsync(CreateTodoDto dto)
    {
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(item);
        return ToDto(item);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTodoDto dto)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null) return false;

        item.Title = dto.Title;
        item.IsDone = dto.IsDone;
        await repository.UpdateAsync(item);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item is null) return false;

        await repository.DeleteAsync(id);
        return true;
    }

    private static TodoDto ToDto(TodoItem item) =>
        new(item.Id, item.Title, item.IsDone, item.CreatedAt);
}
