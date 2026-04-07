namespace SQLiteNetExtensions.Data.Sqlite.Attributes;

public class OneToManyAttribute(string? inverseForeignKey = null, string? inverseProperty = null) : RelationshipAttribute(null, inverseForeignKey, inverseProperty)
{
}
