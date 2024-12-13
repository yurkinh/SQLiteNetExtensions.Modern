using System.Text.Json;

namespace SQLiteNetExtensions.Extensions.TextBlob.Serializers;

public class JsonBlobSerializer : ITextBlobSerializer
{
	public string Serialize(object element) => JsonSerializer.Serialize(element);

	public object? Deserialize(string text, Type type) => JsonSerializer.Deserialize(text, type);

}