using System.Collections;
using Microsoft.Data.Sqlite;

namespace SQLiteNetExtensions.MicrosoftData.MicroOrm;

internal static class SqliteConnectionAsyncExtensions
{
	public static Task<T> GetAsync<T>(this SqliteConnection conn, object pk) where T : new()
	{
		return Task.Run(() => conn.Get<T>(pk));
	}

	public static Task<T?> FindAsync<T>(this SqliteConnection conn, object pk) where T : new()
	{
		return Task.Run(() => conn.Find<T>(pk));
	}

	public static Task<int> InsertAsync(this SqliteConnection conn, object element)
	{
		return Task.Run(() => conn.Insert(element));
	}

	public static Task<int> InsertOrReplaceAsync(this SqliteConnection conn, object element)
	{
		return Task.Run(() => conn.InsertOrReplace(element));
	}

	public static Task<int> InsertAllAsync(this SqliteConnection conn, IEnumerable elements)
	{
		return Task.Run(() => conn.InsertAll(elements));
	}

	public static Task<int> UpdateAsync(this SqliteConnection conn, object element)
	{
		return Task.Run(() => conn.Update(element));
	}

	public static Task<int> DeleteAsync(this SqliteConnection conn, object element)
	{
		return Task.Run(() => conn.Delete(element));
	}

	public static Task<int> ExecuteAsync(this SqliteConnection conn, string sql, params object[] args)
	{
		return Task.Run(() => conn.Execute(sql, args));
	}

	public static Task<List<object>> QueryAsync(this SqliteConnection conn, TableMapping mapping, string sql, params object[] args)
	{
		return Task.Run(() => conn.Query(mapping, sql, args).ToList());
	}

	public static Task<TableMapping> GetMappingAsync(this SqliteConnection conn, Type type)
	{
		return Task.Run(() => conn.GetMapping(type));
	}

	public static Task CreateTableAsync<T>(this SqliteConnection conn) where T : new()
	{
		return Task.Run(() => conn.CreateTable<T>());
	}
}
