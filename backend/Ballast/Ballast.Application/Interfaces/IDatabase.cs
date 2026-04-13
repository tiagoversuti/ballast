using System.Data;

namespace Ballast.Application.Interfaces;

public interface IDatabase
{
    Task<List<T>> QueryAsync<T>(string sql, IReadOnlyDictionary<string, object> parameters, Func<IDataRecord, T> map);
    Task ExecuteAsync(string sql, IReadOnlyDictionary<string, object> parameters);
    Task<int> ScalarIntAsync(string sql, IReadOnlyDictionary<string, object> parameters);
}
