namespace SQLiteNetExtensions.Data.Sqlite.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute(string name) : Attribute
{
	public string Name { get; private set; } = name;
}
