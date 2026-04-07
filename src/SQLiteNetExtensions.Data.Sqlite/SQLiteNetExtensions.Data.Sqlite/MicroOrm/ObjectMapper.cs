using System.Reflection;
using Microsoft.Data.Sqlite;

namespace SQLiteNetExtensions.Data.Sqlite.MicroOrm;

internal static class ObjectMapper
{
	public static T MapReaderToObject<T>(SqliteDataReader reader) where T : new()
	{
		return (T)MapReaderToObject(reader, typeof(T));
	}

	public static object MapReaderToObject(SqliteDataReader reader, Type type)
	{
		var mapping = TableMapping.Get(type);
		return MapReaderToObject(reader, mapping);
	}

	public static object MapReaderToObject(SqliteDataReader reader, TableMapping mapping)
	{
		var obj = Activator.CreateInstance(mapping.MappedType)!;

		for (int i = 0; i < reader.FieldCount; i++)
		{
			var colName = reader.GetName(i);
			var column = FindColumn(mapping, colName);
			if (column == null)
			{
				continue;
			}

			if (reader.IsDBNull(i))
			{
				column.SetValue(obj, null);
				continue;
			}

			var value = reader.GetValue(i);
			var converted = ConvertValue(value, column.PropertyInfo.PropertyType);
			column.SetValue(obj, converted);
		}

		return obj;
	}

	static TableMapping.Column? FindColumn(TableMapping mapping, string colName)
	{
		foreach (var col in mapping.Columns)
		{
			if (string.Equals(col.Name, colName, StringComparison.OrdinalIgnoreCase))
			{
				return col;
			}
		}
		return null;
	}

	public static object? ConvertValue(object? value, Type targetType)
	{
		if (value == null || value == DBNull.Value)
		{
			return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
		}

		var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

		if (underlyingType == value.GetType())
		{
			return value;
		}

		if (underlyingType == typeof(bool))
		{
			return Convert.ToInt64(value) != 0;
		}

		if (underlyingType.IsEnum)
		{
			return Enum.ToObject(underlyingType, Convert.ToInt64(value));
		}

		if (underlyingType == typeof(Guid))
		{
			return value is string s ? Guid.Parse(s) : value is byte[] b ? new Guid(b) : value;
		}

		if (underlyingType == typeof(DateTime))
		{
			return value is string ds ? DateTime.Parse(ds) : Convert.ToDateTime(value);
		}

		if (underlyingType == typeof(DateTimeOffset))
		{
			return value is string dtos ? DateTimeOffset.Parse(dtos) : new DateTimeOffset(Convert.ToDateTime(value));
		}

		if (underlyingType == typeof(TimeSpan))
		{
			return value is string tss ? TimeSpan.Parse(tss) : TimeSpan.FromTicks(Convert.ToInt64(value));
		}

		if (underlyingType == typeof(byte[]))
		{
			return value is byte[] bytes ? bytes : value;
		}

		return Convert.ChangeType(value, underlyingType);
	}

	public static string GetSqlType(Type clrType)
	{
		var underlying = Nullable.GetUnderlyingType(clrType) ?? clrType;

		if (underlying == typeof(bool) ||
			underlying == typeof(byte) || underlying == typeof(sbyte) ||
			underlying == typeof(short) || underlying == typeof(ushort) ||
			underlying == typeof(int) || underlying == typeof(uint) ||
			underlying == typeof(long) || underlying == typeof(ulong) ||
			underlying.IsEnum)
		{
			return "integer";
		}

		if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
		{
			return "real";
		}

		if (underlying == typeof(byte[]))
		{
			return "blob";
		}

		// string, DateTime, Guid, TimeSpan, DateTimeOffset all stored as text
		return "text";
	}
}
