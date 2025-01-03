namespace SQLiteNetExtensions.Attributes;

public class ManyToManyAttribute(Type intermediateType, string? inverseForeignKey = null, string? inverseProperty = null) : RelationshipAttribute(null, inverseForeignKey, inverseProperty)
{
	public Type IntermediateType { get; private set; } = intermediateType;
}