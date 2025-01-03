namespace SQLiteNetExtensions.Exceptions;

public class IncorrectRelationshipException(string typeName, string propertyName, string message) : Exception(string.Format("{0}.{1}: {2}", typeName, propertyName, message))
{
	public string? PropertyName { get; set; }
	public string? TypeName { get; set; }
}

