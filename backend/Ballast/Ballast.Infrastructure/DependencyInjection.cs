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
        services.AddScoped<IDatabase>(db => new SqlDatabase(connectionString));
        services.AddScoped<ITodoRepository>(r => new TodoRepository(r.GetRequiredService<IDatabase>()));
        services.AddScoped<IUserRepository>(r => new UserRepository(r.GetRequiredService<IDatabase>()));
        return services;
    }
}
