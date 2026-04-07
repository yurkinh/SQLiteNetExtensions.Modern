using System.Collections;
using Microsoft.Data.Sqlite;

namespace SQLiteNetExtensions.MicrosoftData.MicroOrm;

internal static class SqliteConnectionExtensions
{
	static void EnsureOpen(SqliteConnection conn)
	{
		if (conn.State != System.Data.ConnectionState.Open)
		{
			conn.Open();
		}
	}

	public static TableMapping GetMapping(this SqliteConnection conn, Type type)
	{
		return TableMapping.Get(type);
	}

	public static void CreateTable<T>(this SqliteConnection conn) where T : new()
	{
		conn.CreateTable(typeof(T));
	}

	public static void CreateTable(this SqliteConnection conn, Type type)
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(type);

		var columns = new List<string>();
		foreach (var col in mapping.Columns)
		{
			var parts = $"[{col.Name}] {ObjectMapper.GetSqlType(col.ColumnType)}";
			if (col.IsPK)
			{
				parts += " primary key";
			}
			if (col.IsAutoInc)
			{
				parts += " autoincrement";
			}
			columns.Add(parts);
		}

		var sql = $"create table if not exists [{mapping.TableName}] ({string.Join(", ", columns)})";
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.ExecuteNonQuery();
	}

	public static int Insert(this SqliteConnection conn, object element)
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(element.GetType());
		var columnsToInsert = mapping.Columns.Where(c => !c.IsAutoInc).ToArray();

		var colNames = string.Join(", ", columnsToInsert.Select(c => $"[{c.Name}]"));
		var colParams = string.Join(", ", columnsToInsert.Select((_, i) => $"@p{i}"));
		var sql = $"insert into [{mapping.TableName}] ({colNames}) values ({colParams})";

		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		for (int i = 0; i < columnsToInsert.Length; i++)
		{
			var value = columnsToInsert[i].GetValue(element);
			cmd.Parameters.AddWithValue($"@p{i}", SerializeValue(value));
		}

		var result = cmd.ExecuteNonQuery();

		// Set auto-increment PK back on the object
		if (mapping.PK is { IsAutoInc: true })
		{
			using var lastIdCmd = conn.CreateCommand();
			lastIdCmd.CommandText = "select last_insert_rowid()";
			var lastId = lastIdCmd.ExecuteScalar();
			if (lastId != null)
			{
				var converted = ObjectMapper.ConvertValue(lastId, mapping.PK.PropertyInfo.PropertyType);
				mapping.PK.SetValue(element, converted);
			}
		}

		return result;
	}

	public static int InsertOrReplace(this SqliteConnection conn, object element)
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(element.GetType());
		var columnsToInsert = mapping.Columns;

		var colNames = string.Join(", ", columnsToInsert.Select(c => $"[{c.Name}]"));
		var colParams = string.Join(", ", columnsToInsert.Select((_, i) => $"@p{i}"));
		var sql = $"insert or replace into [{mapping.TableName}] ({colNames}) values ({colParams})";

		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		for (int i = 0; i < columnsToInsert.Length; i++)
		{
			var value = columnsToInsert[i].GetValue(element);
			cmd.Parameters.AddWithValue($"@p{i}", SerializeValue(value));
		}

		var result = cmd.ExecuteNonQuery();

		// Set auto-increment PK back on the object (for insert case)
		if (mapping.PK is { IsAutoInc: true })
		{
			using var lastIdCmd = conn.CreateCommand();
			lastIdCmd.CommandText = "select last_insert_rowid()";
			var lastId = lastIdCmd.ExecuteScalar();
			if (lastId != null)
			{
				var converted = ObjectMapper.ConvertValue(lastId, mapping.PK.PropertyInfo.PropertyType);
				mapping.PK.SetValue(element, converted);
			}
		}

		return result;
	}

	public static int InsertAll(this SqliteConnection conn, IEnumerable elements)
	{
		EnsureOpen(conn);
		int count = 0;
		foreach (var element in elements)
		{
			conn.Insert(element);
			count++;
		}
		return count;
	}

	public static int Update(this SqliteConnection conn, object element)
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(element.GetType());
		if (mapping.PK == null)
		{
			throw new InvalidOperationException($"Cannot update objects of type {element.GetType().Name} without a primary key");
		}

		var nonPkColumns = mapping.Columns.Where(c => !c.IsPK).ToArray();
		var setClause = string.Join(", ", nonPkColumns.Select((c, i) => $"[{c.Name}] = @p{i}"));
		var sql = $"update [{mapping.TableName}] set {setClause} where [{mapping.PK.Name}] = @pk";

		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		for (int i = 0; i < nonPkColumns.Length; i++)
		{
			var value = nonPkColumns[i].GetValue(element);
			cmd.Parameters.AddWithValue($"@p{i}", SerializeValue(value));
		}

		cmd.Parameters.AddWithValue("@pk", SerializeValue(mapping.PK.GetValue(element)));
		return cmd.ExecuteNonQuery();
	}

	public static int Delete(this SqliteConnection conn, object element)
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(element.GetType());
		if (mapping.PK == null)
		{
			throw new InvalidOperationException($"Cannot delete objects of type {element.GetType().Name} without a primary key");
		}

		var sql = $"delete from [{mapping.TableName}] where [{mapping.PK.Name}] = @pk";
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.Parameters.AddWithValue("@pk", SerializeValue(mapping.PK.GetValue(element)));
		return cmd.ExecuteNonQuery();
	}

	public static T Get<T>(this SqliteConnection conn, object pk) where T : new()
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(typeof(T));

		var sql = $"select * from [{mapping.TableName}] where [{mapping.PK.Name}] = @pk";
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.Parameters.AddWithValue("@pk", SerializeValue(pk));

		using var reader = cmd.ExecuteReader();
		if (reader.Read())
		{
			return ObjectMapper.MapReaderToObject<T>(reader);
		}

		throw new InvalidOperationException($"Object of type {typeof(T).Name} with primary key {pk} not found");
	}

	public static T? Find<T>(this SqliteConnection conn, object pk) where T : new()
	{
		EnsureOpen(conn);
		var mapping = TableMapping.Get(typeof(T));

		var sql = $"select * from [{mapping.TableName}] where [{mapping.PK.Name}] = @pk";
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.Parameters.AddWithValue("@pk", SerializeValue(pk));

		using var reader = cmd.ExecuteReader();
		if (reader.Read())
		{
			return ObjectMapper.MapReaderToObject<T>(reader);
		}

		return default;
	}

	public static TableQuery<T> Table<T>(this SqliteConnection conn) where T : new()
	{
		return new TableQuery<T>(conn);
	}

	public static int Execute(this SqliteConnection conn, string sql, params object[] args)
	{
		EnsureOpen(conn);
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		BindArgs(cmd, args);
		return cmd.ExecuteNonQuery();
	}

	public static IList<object> Query(this SqliteConnection conn, TableMapping mapping, string sql, params object[] args)
	{
		EnsureOpen(conn);
		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		BindArgs(cmd, args);

		var results = new List<object>();
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			results.Add(ObjectMapper.MapReaderToObject(reader, mapping));
		}

		return results;
	}

	public static void DeleteAllIds(this SqliteConnection conn, object[] primaryKeyValues, string entityName, string primaryKeyName)
	{
		if (primaryKeyValues == null || primaryKeyValues.Length == 0)
		{
			return;
		}

		EnsureOpen(conn);
		var placeholders = string.Join(",", primaryKeyValues.Select((_, i) => $"@p{i}"));
		var sql = $"delete from [{entityName}] where [{primaryKeyName}] in ({placeholders})";

		using var cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		for (int i = 0; i < primaryKeyValues.Length; i++)
		{
			cmd.Parameters.AddWithValue($"@p{i}", SerializeValue(primaryKeyValues[i]));
		}

		cmd.ExecuteNonQuery();
	}

	static void BindArgs(SqliteCommand cmd, object[] args)
	{
		// sqlite-net uses positional parameters with "?"
		// Replace ? placeholders with named params
		if (args.Length == 0)
		{
			return;
		}

		int paramIndex = 0;
		var newCommandText = new System.Text.StringBuilder(cmd.CommandText.Length + args.Length * 4);
		foreach (char c in cmd.CommandText)
		{
			if (c == '?' && paramIndex < args.Length)
			{
				var paramName = $"@arg{paramIndex}";
				newCommandText.Append(paramName);
				cmd.Parameters.AddWithValue(paramName, SerializeValue(args[paramIndex]));
				paramIndex++;
			}
			else
			{
				newCommandText.Append(c);
			}
		}

		cmd.CommandText = newCommandText.ToString();
	}

	static object SerializeValue(object? value)
	{
		if (value == null)
		{
			return DBNull.Value;
		}

		var type = value.GetType();
		var underlying = Nullable.GetUnderlyingType(type) ?? type;

		if (underlying == typeof(bool))
		{
			return (bool)value ? 1L : 0L;
		}

		if (underlying.IsEnum)
		{
			return Convert.ToInt64(value);
		}

		if (underlying == typeof(Guid))
		{
			return value.ToString()!;
		}

		if (underlying == typeof(DateTime))
		{
			return ((DateTime)value).ToString("O");
		}

		if (underlying == typeof(DateTimeOffset))
		{
			return ((DateTimeOffset)value).ToString("O");
		}

		if (underlying == typeof(TimeSpan))
		{
			return ((TimeSpan)value).Ticks;
		}

		return value;
	}
}
