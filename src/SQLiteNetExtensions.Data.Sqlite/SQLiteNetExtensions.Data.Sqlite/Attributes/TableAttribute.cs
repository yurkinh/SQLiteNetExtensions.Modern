namespace SQLiteNetExtensions.Data.Sqlite.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name) : Attribute
{
	public string Name { get; private set; } = name;
}
