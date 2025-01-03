
namespace SQLiteNetExtensions.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class TextBlobAttribute(string textProperty) : RelationshipAttribute(null, null, null)
{
	public string TextProperty { get; private set; } = textProperty;

	// No cascade operations allowed on TextBlob properties
	public override CascadeOperation CascadeOperations { get { return CascadeOperation.None; } }
}
