using System.Diagnostics;
using System.Reflection;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions.TextBlob.Serializers;

namespace SQLiteNetExtensions.Extensions.TextBlob;

public static class TextBlobOperations
{
	static ITextBlobSerializer? serializer;

	public static void SetTextSerializer(ITextBlobSerializer serializer)
	{
		TextBlobOperations.serializer = serializer;
	}

	public static ITextBlobSerializer GetTextSerializer()
	{
		// If not specified, use default JSON serializer
		return serializer ??= new JsonBlobSerializer();
	}

	public static void GetTextBlobChild(object element, PropertyInfo relationshipProperty)
	{
		if (element is null || relationshipProperty is null)
		{
			throw new ArgumentNullException(element is null ? nameof(element) : nameof(relationshipProperty));
		}

		if (relationshipProperty.PropertyType == typeof(string))
		{
			throw new InvalidOperationException("TextBlob property cannot be a string");
		}

		var textblobAttribute = relationshipProperty.GetAttribute<TextBlobAttribute>();
		var textProperty = element.GetType().GetRuntimeProperty(textblobAttribute!.TextProperty)
			?? throw new InvalidOperationException($"Text property '{textblobAttribute.TextProperty}' not found");

		if (textProperty.PropertyType != typeof(string))
		{
			throw new InvalidOperationException("Text property must be of type string");
		}

		if (textProperty.GetValue(element) is string textValue)
		{
			var value = GetTextSerializer().Deserialize(textValue, relationshipProperty.PropertyType);
			relationshipProperty.SetValue(element, value, null);
		}
		else
		{
			relationshipProperty.SetValue(element, null, null);
		}
	}

	public static void UpdateTextBlobProperty(object element, PropertyInfo relationshipProperty)
	{
		if (element is null || relationshipProperty is null)
		{
			throw new ArgumentNullException(element is null ? nameof(element) : nameof(relationshipProperty));
		}

		if (relationshipProperty.PropertyType == typeof(string))
		{
			throw new InvalidOperationException("TextBlob property cannot be a string");
		}

		var textblobAttribute = relationshipProperty.GetAttribute<TextBlobAttribute>();
		var textProperty = element.GetType().GetRuntimeProperty(textblobAttribute!.TextProperty)
			?? throw new InvalidOperationException($"Text property '{textblobAttribute.TextProperty}' not found");

		if (textProperty.PropertyType != typeof(string))
		{
			throw new InvalidOperationException("Text property must be of type string");
		}

		var value = relationshipProperty.GetValue(element);
		var textValue = value is null ? null : GetTextSerializer().Serialize(value);

		textProperty.SetValue(element, textValue);
	}
}
