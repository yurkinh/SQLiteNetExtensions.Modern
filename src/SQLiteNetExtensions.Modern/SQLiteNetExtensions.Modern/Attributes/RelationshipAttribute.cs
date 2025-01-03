using SQLite;

namespace SQLiteNetExtensions.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class RelationshipAttribute(string? foreignKey, string? inverseForeignKey, string? inverseProperty) : IgnoreAttribute
{
	public string? ForeignKey { get; private set; } = foreignKey;
	public string? InverseProperty { get; private set; } = inverseProperty;
	public string? InverseForeignKey { get; private set; } = inverseForeignKey;
	public virtual CascadeOperation CascadeOperations { get; set; }
	public bool ReadOnly { get; set; }

	public bool IsCascadeRead { get { return CascadeOperations.HasFlag(CascadeOperation.CascadeRead); } }
	public bool IsCascadeInsert { get { return CascadeOperations.HasFlag(CascadeOperation.CascadeInsert); } }
	public bool IsCascadeDelete { get { return CascadeOperations.HasFlag(CascadeOperation.CascadeDelete); } }
}
