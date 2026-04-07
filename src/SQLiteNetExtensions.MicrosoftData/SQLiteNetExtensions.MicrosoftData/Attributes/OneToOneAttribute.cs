namespace SQLiteNetExtensions.MicrosoftData.Attributes;

public class OneToOneAttribute(string? foreignKey = null, string? inverseProperty = null) : RelationshipAttribute(foreignKey, null, inverseProperty)
{
}
