using Ballast.Application.Interfaces;
using Ballast.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Ballast.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<ITodoRepository>(_ => new TodoRepository(connectionString));
        services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
        return services;
    }
}
