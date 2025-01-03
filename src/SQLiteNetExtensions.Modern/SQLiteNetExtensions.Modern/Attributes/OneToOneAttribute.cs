namespace SQLiteNetExtensions.Attributes;

public class OneToOneAttribute(string? foreignKey = null, string? inverseProperty = null) : RelationshipAttribute(foreignKey, null, inverseProperty)
{
}