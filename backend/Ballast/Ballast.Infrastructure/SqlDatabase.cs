using System.Data;
using Ballast.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace Ballast.Infrastructure;

public class SqlDatabase(string connectionString) : IDatabase
{
    public async Task<List<T>> QueryAsync<T>(string sql, IReadOnlyDictionary<string, object> parameters, Func<IDataRecord, T> map)
    {
        var results = new List<T>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(map(reader));

        return results;
    }

    public async Task ExecuteAsync(string sql, IReadOnlyDictionary<string, object> parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> ScalarIntAsync(string sql, IReadOnlyDictionary<string, object> parameters)
    {
        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        await connection.OpenAsync();
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
