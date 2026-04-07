using System.Collections.Concurrent;
using System.Reflection;
using SQLiteNetExtensions.MicrosoftData.Attributes;

namespace SQLiteNetExtensions.MicrosoftData.MicroOrm;

internal class TableMapping
{
	static readonly ConcurrentDictionary<Type, TableMapping> cache = new();

	public Type MappedType { get; }
	public string TableName { get; }
	public Column[] Columns { get; }
	public Column PK { get; }

	TableMapping(Type type)
	{
		MappedType = type;

		var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();
		TableName = tableAttribute?.Name ?? type.Name;

		var columns = new List<Column>();
		Column? pk = null;

		foreach (var prop in type.GetRuntimeProperties())
		{
			if (!IsPublicInstance(prop))
			{
				continue;
			}

			if (prop.GetCustomAttribute<IgnoreAttribute>() != null)
			{
				continue;
			}

			var col = new Column(prop);
			columns.Add(col);

			if (col.IsPK)
			{
				pk = col;
			}
		}

		Columns = [.. columns];
		PK = pk!;
	}

	public static TableMapping Get(Type type)
	{
		return cache.GetOrAdd(type, t => new TableMapping(t));
	}

	static bool IsPublicInstance(PropertyInfo propertyInfo)
	{
		var getter = propertyInfo.GetMethod;
		var setter = propertyInfo.SetMethod;

		return getter is { IsPublic: true, IsStatic: false } &&
			   setter is { IsPublic: true, IsStatic: false };
	}

	internal class Column
	{
		public string Name { get; }
		public PropertyInfo PropertyInfo { get; }
		public bool IsPK { get; }
		public bool IsAutoInc { get; }
		public Type ColumnType { get; }

		public Column(PropertyInfo prop)
		{
			PropertyInfo = prop;

			var columnAttribute = prop.GetCustomAttribute<ColumnAttribute>();
			Name = columnAttribute?.Name ?? prop.Name;

			IsPK = prop.GetCustomAttribute<PrimaryKeyAttribute>() != null;
			IsAutoInc = prop.GetCustomAttribute<AutoIncrementAttribute>() != null;
			ColumnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
		}

		public object? GetValue(object obj) => PropertyInfo.GetValue(obj);
		public void SetValue(object obj, object? value) => PropertyInfo.SetValue(obj, value);
	}
}
