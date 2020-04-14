using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite;

namespace RATBVFormsPrism.Services
{
    /// <summary>
    /// Interface extracted from the source code of SQLiteAsyncConnection. 
    /// Original class at https://github.com/oysteinkrog/SQLite.Net-PCL/blob/master/src/SQLite.Net.Async/SQLiteAsyncConnection.cs
    /// Github repo at https://gist.github.com/seanfisher/cb51e149575da077103700a088e9394e
    /// MODIFIED BY WARBORG to suit the SQLiteNetExtensions Nuget we are using
    /// Example of using at https://github.com/oysteinkrog/SQLite.Net-PCL/issues/188
    /// </summary>
    public interface ISQLiteAsyncConnection
    {
        SQLiteConnectionWithLock GetConnection();

        Task<CreateTablesResult> CreateTableAsync<T>(CreateFlags createFlags = CreateFlags.None)
            where T : new();

        Task<CreateTablesResult> CreateTablesAsync<T, T2>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new();

        Task<CreateTablesResult> CreateTablesAsync<T, T2, T3>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
            where T3 : new();

        Task<CreateTablesResult> CreateTablesAsync<T, T2, T3, T4>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
            where T3 : new()
            where T4 : new();

        Task<CreateTablesResult> CreateTablesAsync<T, T2, T3, T4, T5>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new();

        Task<CreateTablesResult> CreateTablesAsync(CreateFlags createFlags = CreateFlags.None, params Type[] types);

        Task<int> DropTableAsync<T>()
            where T : new();

        Task<int> InsertAsync(object item);
        Task<int> UpdateAsync(object item);
        Task<int> InsertOrReplaceAsync(object item);
        Task<int> DeleteAsync(object item);

        Task<T> GetAsync<T>(object pk)
            where T : new();

        Task<T> FindAsync<T>(object pk)
            where T : new();

        Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate)
            where T : new();

        Task<T> FindAsync<T>(Expression<Func<T, bool>> predicate)
            where T : new();

        Task<int> ExecuteAsync(string query, params object[] args);
        Task<int> InsertAllAsync(IEnumerable items);
        Task<int> UpdateAllAsync(IEnumerable items);
        Task RunInTransactionAsync(Action<SQLiteAsyncConnection> action);
        Task RunInTransactionAsync(Action<SQLiteConnection> action);

        AsyncTableQuery<T> Table<T>()
            where T : new();

        Task<T> ExecuteScalarAsync<T>(string sql, params object[] args);

        Task<List<T>> QueryAsync<T>(string sql, params object[] args)
            where T : new();
    }
}