using System.Diagnostics;
using System.Reflection;
using SQLiteNetExtensions.Attributes;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using SQLite;

namespace SQLiteNetExtensions.Extensions;

public enum EnclosedType
{
	None,
	Array,
	List,
	ObservableCollection
}

public class ManyToManyMetaInfo
{
	public Type IntermediateType { get; set; }
	public PropertyInfo OriginProperty { get; set; }
	public PropertyInfo DestinationProperty { get; set; }
}

public static class ReflectionExtensions
{
	public static T GetAttribute<T>(this Type type) where T : Attribute
	{
		T attribute = null;
		var attributes = (T[])type.GetTypeInfo().GetCustomAttributes(typeof(T), true);
		if (attributes.Length > 0)
		{
			attribute = attributes[0];
		}
		return attribute;
	}

	public static T GetAttribute<T>(this PropertyInfo property) where T : Attribute
	{
		T attribute = null;
		var attributes = (T[])property.GetCustomAttributes(typeof(T), true);
		if (attributes.Length > 0)
		{
			attribute = attributes[0];
		}
		return attribute;
	}

	public static Type GetEntityType(this PropertyInfo property, out EnclosedType enclosedType)
	{
		var type = property.PropertyType;
		enclosedType = EnclosedType.None;

		var typeInfo = type.GetTypeInfo();
		if (type.IsArray)
		{
			type = type.GetElementType();
			enclosedType = EnclosedType.Array;
		}
		else if (typeInfo.IsGenericType && typeof(List<>).GetTypeInfo().IsAssignableFrom(type.GetGenericTypeDefinition().GetTypeInfo()))
		{
			type = typeInfo.GenericTypeArguments[0];
			enclosedType = EnclosedType.List;
		}
		else if (typeInfo.IsGenericType && typeof(ObservableCollection<>).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo().GetGenericTypeDefinition().GetTypeInfo()))
		{
			type = typeInfo.GenericTypeArguments[0];
			enclosedType = EnclosedType.ObservableCollection;
		}
		return type;
	}

	public static object? GetDefault(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
	}

	static PropertyInfo? GetExplicitForeignKeyProperty(this Type type, Type destinationType)
	{
		ArgumentNullException.ThrowIfNull(type);
		ArgumentNullException.ThrowIfNull(destinationType);

		return type.GetRuntimeProperties()
			.Where(p => p.IsPublicInstance())
			.Select(p => (Property: p, ForeignKey: p.GetAttribute<ForeignKeyAttribute>()))
			.Where(t => t.ForeignKey?.ForeignType.GetTypeInfo().IsAssignableFrom(destinationType.GetTypeInfo()) == true)
			.Select(t => t.Property)
			.FirstOrDefault();
	}

	static readonly string[] conventionFormats = ["{0}Id", "{0}Key", "{0}ForeignKey"];

	static PropertyInfo? GetConventionForeignKeyProperty(this Type type, string destinationTypeName)
	{
		ArgumentNullException.ThrowIfNull(type);
		ArgumentException.ThrowIfNullOrEmpty(destinationTypeName);

		var conventionNames = new HashSet<string>(
			conventionFormats.Select(format => string.Format(format, destinationTypeName)),
			StringComparer.OrdinalIgnoreCase
		);

		return type.GetRuntimeProperties()
			.FirstOrDefault(property =>
				property.IsPublicInstance() &&
				conventionNames.Contains(property.Name));
	}

	public static PropertyInfo GetForeignKeyProperty(this Type type, PropertyInfo relationshipProperty, Type? intermediateType = null, bool inverse = false)
	{
		ArgumentNullException.ThrowIfNull(type);
		ArgumentNullException.ThrowIfNull(relationshipProperty);

		var attribute = relationshipProperty.GetAttribute<RelationshipAttribute>();
		var propertyType = relationshipProperty.GetEntityType(out _);

		var originType = intermediateType ?? (inverse ? propertyType : type);
		var destinationType = inverse ? type : propertyType;

		var inverseProperty = type.GetInverseProperty(relationshipProperty);
		var inverseAttribute = inverseProperty?.GetAttribute<RelationshipAttribute>();

		// Resolve foreign key property name based on attributes
		string? foreignKeyName = ResolveForeignKeyName(attribute, inverseAttribute, inverse);

		if (!string.IsNullOrEmpty(foreignKeyName))
		{
			return originType.GetRuntimeProperty(foreignKeyName);
		}

		// Fall back to convention-based lookup
		return originType.GetExplicitForeignKeyProperty(destinationType) ??
		       originType.GetConventionForeignKeyProperty(destinationType.Name);
	}

	static string? ResolveForeignKeyName(
		RelationshipAttribute attribute,
		RelationshipAttribute? inverseAttribute,
		bool inverse)
	{
		return inverse switch
		{
			false when !string.IsNullOrEmpty(attribute.ForeignKey) => attribute.ForeignKey,
			false when inverseAttribute?.InverseForeignKey is not null => inverseAttribute.InverseForeignKey,
			true when !string.IsNullOrEmpty(attribute.InverseForeignKey) => attribute.InverseForeignKey,
			true when inverseAttribute?.ForeignKey is not null => inverseAttribute.ForeignKey,
			_ => null
		};
	}


	public static PropertyInfo? GetInverseProperty(this Type elementType, PropertyInfo property)
	{
		ArgumentNullException.ThrowIfNull(elementType);
		ArgumentNullException.ThrowIfNull(property);

		var attribute = property.GetAttribute<RelationshipAttribute>();
		if (attribute is null or { InverseProperty: "" })
		{
			return null;
		}

		var propertyType = property.GetEntityType(out _);

		// If inverse property is explicitly specified, return it
		if (!string.IsNullOrEmpty(attribute.InverseProperty))
		{
			return propertyType.GetRuntimeProperty(attribute.InverseProperty);
		}

		// Otherwise search for matching property by type
		return propertyType.GetRuntimeProperties()
			.Where(p => p.IsPublicInstance())
			.FirstOrDefault(p =>
			{
				if (p.GetAttribute<RelationshipAttribute>() is not { } inverseAttribute)
				{
					return false;
				}

				var inverseType = p.GetEntityType(out _);
				return inverseType.GetTypeInfo().Equals(elementType.GetTypeInfo());
			});
	}

	public static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expression)
	{
		var type = typeof(T);
		var body = expression.Body as MemberExpression;
		Debug.Assert(body != null, "Expression should be a property member expression");

		var propertyName = body.Member.Name;
		return type.GetRuntimeProperty(propertyName);
	}

	public static ManyToManyMetaInfo GetManyToManyMetaInfo(this Type type, PropertyInfo relationship)
	{
		var manyToManyAttribute = relationship.GetAttribute<ManyToManyAttribute>();
		Debug.Assert(manyToManyAttribute != null, "Unable to find ManyToMany attribute");

		var intermediateType = manyToManyAttribute.IntermediateType;
		var destinationKeyProperty = type.GetForeignKeyProperty(relationship, intermediateType);
		var inverseKeyProperty = type.GetForeignKeyProperty(relationship, intermediateType, true);

		return new ManyToManyMetaInfo
		{
			IntermediateType = intermediateType,
			OriginProperty = inverseKeyProperty,
			DestinationProperty = destinationKeyProperty
		};
	}

	public static List<PropertyInfo> GetRelationshipProperties(this Type type)
	{
		return (from property in type.GetRuntimeProperties()
				where property.IsPublicInstance() && property.GetAttribute<RelationshipAttribute>() != null
				select property).ToList();
	}

	public static PropertyInfo? GetPrimaryKey(this Type type)
	{
		ArgumentNullException.ThrowIfNull(type);

		return type.GetRuntimeProperties()
			.Where(property => property.IsPublicInstance())
			.FirstOrDefault(property => property.GetAttribute<PrimaryKeyAttribute>() is not null);
	}

	public static string GetTableName(this Type type)
	{
		var tableName = type.Name;
		var tableAttribute = type.GetAttribute<TableAttribute>();
		if (tableAttribute != null && tableAttribute.Name != null)
		{
			tableName = tableAttribute.Name;
		}

		return tableName;
	}

	public static string GetColumnName(this PropertyInfo property)
	{
		var column = property.Name;
		var columnAttribute = property.GetAttribute<ColumnAttribute>();
		if (columnAttribute != null && columnAttribute.Name != null)
		{
			column = columnAttribute.Name;
		}

		return column;
	}

	// Equivalent to old GetProperties(BindingFlags.Public | BindingFlags.Instance)
	static bool IsPublicInstance(this PropertyInfo propertyInfo)
	{
		ArgumentNullException.ThrowIfNull(propertyInfo);

		var getter = propertyInfo.GetMethod;
		var setter = propertyInfo.SetMethod;

		return getter is { IsPublic: true, IsStatic: false } &&
			   setter is { IsPublic: true, IsStatic: false };
	}
}
