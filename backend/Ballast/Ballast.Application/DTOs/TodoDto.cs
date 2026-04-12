namespace Ballast.Application.DTOs;

public record TodoDto(Guid Id, string Title, bool IsDone, DateTime CreatedAt);
