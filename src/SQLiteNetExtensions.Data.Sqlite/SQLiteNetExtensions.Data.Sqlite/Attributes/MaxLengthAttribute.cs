namespace SQLiteNetExtensions.Data.Sqlite.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class MaxLengthAttribute(int length) : Attribute
{
	public int Value { get; private set; } = length;
}
