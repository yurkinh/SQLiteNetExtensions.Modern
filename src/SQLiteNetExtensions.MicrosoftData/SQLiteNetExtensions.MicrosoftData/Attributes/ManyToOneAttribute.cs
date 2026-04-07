namespace SQLiteNetExtensions.MicrosoftData.Attributes;

public class ManyToOneAttribute(string? foreignKey = null, string? inverseProperty = null) : RelationshipAttribute(foreignKey, null, inverseProperty)
{
}
