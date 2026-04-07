namespace SQLiteNetExtensions.MicrosoftData.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute(Type foreignType) : IndexedAttribute
{
	public Type ForeignType { get; private set; } = foreignType;
}
