using System.Linq.Expressions;
using Microsoft.Data.Sqlite;

namespace SQLiteNetExtensions.Data.Sqlite.MicroOrm;

internal class TableQuery<T> where T : new()
{
	readonly SqliteConnection conn;
	Expression<Func<T, bool>>? filter;

	public TableQuery(SqliteConnection conn)
	{
		this.conn = conn;
	}

	public TableQuery<T> Where(Expression<Func<T, bool>> predicate)
	{
		filter = predicate;
		return this;
	}

	public List<T> ToList()
	{
		var mapping = TableMapping.Get(typeof(T));

		if (conn.State != System.Data.ConnectionState.Open)
		{
			conn.Open();
		}

		var sql = $"select * from [{mapping.TableName}]";
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		var results = new List<T>();
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			results.Add(ObjectMapper.MapReaderToObject<T>(reader));
		}

		// Apply filter in-memory if one was set
		if (filter != null)
		{
			var compiled = filter.Compile();
			results = results.Where(compiled).ToList();
		}

		return results;
	}

	public Task<List<T>> ToListAsync()
	{
		return Task.Run(ToList);
	}

	public int Count()
	{
		if (filter != null)
		{
			return ToList().Count;
		}

		var mapping = TableMapping.Get(typeof(T));

		if (conn.State != System.Data.ConnectionState.Open)
		{
			conn.Open();
		}

		var sql = $"select count(*) from [{mapping.TableName}]";
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		return Convert.ToInt32(cmd.ExecuteScalar());
	}
}
