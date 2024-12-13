namespace SQLiteNetExtensions.Attributes;

    public class OneToManyAttribute(string? inverseForeignKey = null, string? inverseProperty = null) : RelationshipAttribute(null, inverseForeignKey, inverseProperty)
    {
}