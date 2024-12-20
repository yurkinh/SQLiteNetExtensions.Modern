using System.Collections;
using System.Reflection;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Exceptions;
using SQLiteNetExtensions.Extensions.TextBlob;
using SQLite;

namespace SQLiteNetExtensions.Extensions;

public static class WriteOperationsAsync
{
	const int queryLimit = 990; //Make room for extra keys added by the code

	/// <summary>
	/// Enable to allow descriptive error descriptions on incorrect relationships. Enabled by default.
	/// Disable for production environments to remove the checks and reduce performance penalty
	/// </summary>
	public static bool EnableRuntimeAssertions = true;

	/// <summary>
	/// Updates the with foreign keys of the current object and save changes to the database and
	/// updates the inverse foreign keys of the defined relationships so the relationships are
	/// stored correctly in the database. This operation will create or delete the required intermediate
	/// objects for ManyToMany relationships. All related objects must have a primary key assigned in order
	/// to work correctly. This also implies that any object with 'AutoIncrement' primary key must've been
	/// inserted in the database previous to this call.
	/// This method will also update inverse relationships of objects that currently exist in the object tree,
	/// but it won't update inverse relationships of objects that are not reachable through this object
	/// properties. For example, objects removed from a 'ToMany' relationship won't be updated in memory.
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="element">Object to be updated. Must already have been inserted in the database</param>
	public static async Task UpdateWithChildrenAsync(this SQLiteAsyncConnection conn, object element)
	{
		// Update the current element
		RefreshForeignKeys(element);
		await conn.UpdateAsync(element);

		// Update inverse foreign keys
		await conn.UpdateInverseForeignKeysAsync(element);
	}

	/// <summary>
	/// Inserts the element and all the relationships that are annotated with <c>CascadeOperation.CascadeInsert</c>
	/// into the database. If any element already exists in the database a 'Constraint' exception will be raised.
	/// Elements with a primary key that it's not <c>AutoIncrement</c> will need a valid identifier before calling
	/// this method.
	/// If the <c>recursive</c> flag is set to true, all the relationships annotated with
	/// <c>CascadeOperation.CascadeInsert</c> are inserted recursively in the database. This method will handle
	/// loops and inverse relationships correctly. <c>ReadOnly</c> properties will be omitted.
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="element">Object to be inserted.</param>
	/// <param name="recursive">If set to <c>true</c> all the insert-cascade properties will be inserted</param>
	public static Task InsertWithChildrenAsync(this SQLiteAsyncConnection conn, object element, bool recursive = false)
	{
		return conn.InsertWithChildrenRecursiveAsync(element, false, recursive);
	}

	/// <summary>
	/// Inserts or replace the element and all the relationships that are annotated with
	/// <c>CascadeOperation.CascadeInsert</c> into the database. If any element already exists in the database
	/// it will be replaced. Elements with <c>AutoIncrement</c> primary keys that haven't been assigned will
	/// be always inserted instead of replaced.
	/// If the <c>recursive</c> flag is set to true, all the relationships annotated with
	/// <c>CascadeOperation.CascadeInsert</c> are inserted recursively in the database. This method will handle
	/// loops and inverse relationships correctly. <c>ReadOnly</c> properties will be omitted.
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="element">Object to be inserted.</param>
	/// <param name="recursive">If set to <c>true</c> all the insert-cascade properties will be inserted</param>
	public static Task InsertOrReplaceWithChildrenAsync(this SQLiteAsyncConnection conn, object element, bool recursive = false)
	{
		return conn.InsertWithChildrenRecursiveAsync(element, true, recursive);
	}

	/// <summary>
	/// Inserts all the elements and all the relationships that are annotated with <c>CascadeOperation.CascadeInsert</c>
	/// into the database. If any element already exists in the database a 'Constraint' exception will be raised.
	/// Elements with a primary key that it's not <c>AutoIncrement</c> will need a valid identifier before calling
	/// this method.
	/// If the <c>recursive</c> flag is set to true, all the relationships annotated with
	/// <c>CascadeOperation.CascadeInsert</c> are inserted recursively in the database. This method will handle
	/// loops and inverse relationships correctly. <c>ReadOnly</c> properties will be omitted.
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="elements">Objects to be inserted.</param>
	/// <param name="recursive">If set to <c>true</c> all the insert-cascade properties will be inserted</param>
	public static Task InsertAllWithChildrenAsync(this SQLiteAsyncConnection conn, IEnumerable elements, bool recursive = false)
	{
		return conn.InsertAllWithChildrenRecursiveAsync(elements, false, recursive);
	}

	/// <summary>
	/// Inserts or replace all the elements and all the relationships that are annotated with
	/// <c>CascadeOperation.CascadeInsert</c> into the database. If any element already exists in the database
	/// it will be replaced. Elements with <c>AutoIncrement</c> primary keys that haven't been assigned will
	/// be always inserted instead of replaced.
	/// If the <c>recursive</c> flag is set to true, all the relationships annotated with
	/// <c>CascadeOperation.CascadeInsert</c> are inserted recursively in the database. This method will handle
	/// loops and inverse relationships correctly. <c>ReadOnly</c> properties will be omitted.
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="elements">Objects to be inserted.</param>
	/// <param name="recursive">If set to <c>true</c> all the insert-cascade properties will be inserted</param>
	public static Task InsertOrReplaceAllWithChildrenAsync(this SQLiteAsyncConnection conn, IEnumerable elements, bool recursive = false)
	{
		return conn.InsertAllWithChildrenRecursiveAsync(elements, true, recursive);
	}

	/// <summary>
	/// Deletes all the objects passed as parameters from the database.
	/// If recursive flag is set to true, all relationships marked with 'CascadeDelete' will be
	/// deleted from the database recursively. Inverse relationships and closed entity loops are handled
	/// correctly to avoid endless loops
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="recursive">If set to <c>true</c> all relationships marked with 'CascadeDelete' will be
	/// deleted from the database recursively</param>
	/// <param name="objects">Objects to be deleted from the database</param>
	public static Task DeleteAll(this SQLiteAsyncConnection conn, IEnumerable objects, bool recursive = false)
	{
		return conn.DeleteAllRecursiveAsync(objects, recursive);
	}

	/// <summary>
	/// Deletes all the objects passed as parameters from the database.
	/// If recursive flag is set to true, all relationships marked with 'CascadeDelete' will be
	/// deleted from the database recursively. Inverse relationships and closed entity loops are handled
	/// correctly to avoid endless loops
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="recursive">If set to <c>true</c> all relationships marked with 'CascadeDelete' will be
	/// deleted from the database recursively</param>
	/// <param name="element">Object to be deleted from the database</param>
	public static async Task DeleteAsync(this SQLiteAsyncConnection conn, object element, bool recursive)
	{
		if (recursive)
		{
			await conn.DeleteAll(new[] { element }, recursive);
		}
		else
		{
			await conn.DeleteAsync(element);
		}
	}

	/// <summary>
	/// Deletes all the objects passed with IDs equal to the passed parameters from the database.
	/// Relationships are not taken into account in this method
	/// </summary>
	/// <param name="conn">SQLite Net connection object</param>
	/// <param name="primaryKeyValues">Primary keys of the objects to be deleted from the database</param>
	/// <typeparam name="T">The Entity type, it should match de database entity type</typeparam>
	public static Task DeleteAllIdsAsync<T>(this SQLiteAsyncConnection conn, IEnumerable<object> primaryKeyValues)
	{
		var type = typeof(T);
		var primaryKeyProperty = type.GetPrimaryKey();

		return conn.DeleteAllIdsAsync(primaryKeyValues.ToArray(), type.GetTableName(), primaryKeyProperty.GetColumnName());
	}


	#region Private methods
	static async Task InsertAllWithChildrenRecursiveAsync(this SQLiteAsyncConnection conn, IEnumerable elements, bool replace, bool recursive, ISet<object> objectCache = null)
	{
		if (elements == null)
		{
			return;
		}

		objectCache ??= new HashSet<object>();
		var insertedElements = (await conn.InsertElementsAsync(elements, replace, objectCache)).Cast<object>().ToList();

		foreach (var element in insertedElements)
		{
			await conn.InsertChildrenRecursiveAsync(element, replace, recursive, objectCache);
		}

		foreach (var element in insertedElements)
		{
			await conn.UpdateWithChildrenAsync(element);
		}
	}

	static async Task InsertWithChildrenRecursiveAsync(this SQLiteAsyncConnection conn, object element, bool replace, bool recursive, ISet<object> objectCache = null)
	{
		objectCache ??= new HashSet<object>();
		if (objectCache.Contains(element))
		{
			return;
		}

		await conn.InsertElementAsync(element, replace, objectCache);

		objectCache.Add(element);
		await conn.InsertChildrenRecursiveAsync(element, replace, recursive, objectCache);

		await conn.UpdateWithChildrenAsync(element);
	}

	static async Task InsertChildrenRecursiveAsync(this SQLiteAsyncConnection conn, object element, bool replace, bool recursive, ISet<object> objectCache = null)
	{
		if (element == null)
		{
			return;
		}

		objectCache ??= new HashSet<object>();
		foreach (var relationshipProperty in element.GetType().GetRelationshipProperties())
		{
			var relationshipAttribute = relationshipProperty.GetAttribute<RelationshipAttribute>();

			// Ignore read-only attributes and process only 'CascadeInsert' attributes
			if (relationshipAttribute.ReadOnly || !relationshipAttribute.IsCascadeInsert)
			{
				continue;
			}

			var value = relationshipProperty.GetValue(element, null);
			await conn.InsertValueAsync(value, replace, recursive, objectCache);
		}
	}

	static async Task InsertValueAsync(this SQLiteAsyncConnection conn, object value, bool replace, bool recursive, ISet<object> objectCache)
	{
		if (value == null)
		{
			return;
		}

		var enumerable = value as IEnumerable;
		if (recursive)
		{
			if (enumerable != null)
			{
				await conn.InsertAllWithChildrenRecursiveAsync(enumerable, replace, recursive, objectCache);
			}
			else
			{
				await conn.InsertWithChildrenRecursiveAsync(value, replace, recursive, objectCache);
			}
		}
		else
		{
			if (enumerable != null)
			{
				await conn.InsertElementsAsync(enumerable, replace, objectCache);
			}
			else
			{
				await conn.InsertElementAsync(value, replace, objectCache);
			}
		}
	}

	static async Task<IEnumerable> InsertElementsAsync(this SQLiteAsyncConnection conn, IEnumerable elements, bool replace, ISet<object> objectCache)
	{
		if (elements == null)
		{
			return Enumerable.Empty<object>();
		}

		objectCache ??= new HashSet<object>();
		var elementsToInsert = elements.Cast<object>().Except(objectCache).ToList();
		if (elementsToInsert.Count == 0)
		{
			return Enumerable.Empty<object>();
		}

		var primaryKeyProperty = elementsToInsert[0].GetType().GetPrimaryKey();
		var isAutoIncrementPrimaryKey = primaryKeyProperty != null && primaryKeyProperty.GetAttribute<AutoIncrementAttribute>() != null;

		foreach (var element in elementsToInsert)
		{
			await conn.InsertElementAsync(element, replace, primaryKeyProperty, isAutoIncrementPrimaryKey, objectCache);
			objectCache.Add(element);
		}

		return elementsToInsert;
	}

	static Task InsertElementAsync(this SQLiteAsyncConnection conn, object element, bool replace, ISet<object> objectCache)
	{
		var primaryKeyProperty = element.GetType().GetPrimaryKey();
		var isAutoIncrementPrimaryKey = primaryKeyProperty != null && primaryKeyProperty.GetAttribute<AutoIncrementAttribute>() != null;

		return conn.InsertElementAsync(element, replace, primaryKeyProperty, isAutoIncrementPrimaryKey, objectCache);
	}

	static Task InsertElementAsync(this SQLiteAsyncConnection conn, object element, bool replace, PropertyInfo primaryKeyProperty, bool isAutoIncrementPrimaryKey, ISet<object> objectCache)
	{
		if (element == null || (objectCache != null && objectCache.Contains(element)))
		{
			return Task.CompletedTask;
		}

		bool isPrimaryKeySet = false;
		if (replace && isAutoIncrementPrimaryKey)
		{
			var primaryKeyValue = primaryKeyProperty.GetValue(element, null);
			var defaultPrimaryKeyValue = primaryKeyProperty.PropertyType.GetDefault();
			isPrimaryKeySet = primaryKeyValue != null && !primaryKeyValue.Equals(defaultPrimaryKeyValue);
		}

		bool shouldReplace = replace && (!isAutoIncrementPrimaryKey || isPrimaryKeySet);

		// Only replace elements that have an assigned primary key
		if (shouldReplace)
		{
			return conn.InsertOrReplaceAsync(element);
		}
		else
		{
			return conn.InsertAsync(element);
		}
	}

	static async Task DeleteAllRecursiveAsync(this SQLiteAsyncConnection conn, IEnumerable elements, bool recursive, ISet<object> objectCache = null)
	{
		if (elements == null)
		{
			return;
		}

		var isRootElement = objectCache == null;
		objectCache ??= new HashSet<object>();

		var elementList = elements.Cast<object>().Except(objectCache).ToList();

		// Mark the objects for deletion
		foreach (var element in elementList)
		{
			objectCache.Add(element);
		}

		if (recursive)
		{
			foreach (var element in elementList)
			{
				var type = element.GetType();
				foreach (var relationshipProperty in type.GetRelationshipProperties())
				{
					var relationshipAttribute = relationshipProperty.GetAttribute<RelationshipAttribute>();

					// Ignore read-only attributes or those that are not marked as CascadeDelete
					if (!relationshipAttribute.IsCascadeDelete || relationshipAttribute.ReadOnly)
					{
						continue;
					}

					var value = relationshipProperty.GetValue(element, null);
					await conn.DeleteValueRecursiveAsync(value, recursive, objectCache);
				}
			}
		}

		// To improve performance, the root method call will delete all the objects at once
		if (isRootElement)
		{
			await conn.DeleteAllObjectsAsync(objectCache);
		}
	}

	static Task DeleteValueRecursiveAsync(this SQLiteAsyncConnection conn, object value, bool recursive, ISet<object> objectCache)
	{
		if (value == null)
		{
			return Task.CompletedTask;
		}

		var enumerable = value as IEnumerable ?? new[] { value };
		return conn.DeleteAllRecursiveAsync(enumerable, recursive, objectCache);
	}

	static async Task DeleteAllObjectsAsync(this SQLiteAsyncConnection conn, IEnumerable elements)
	{
		ArgumentNullException.ThrowIfNull(conn);
		if (elements is null)
		{
			return;
		}

		var groupedElements = elements.Cast<object>()
			.GroupBy(o => o.GetType())
			.ToList();

		foreach (var group in groupedElements)
		{
			var type = group.Key;
			var primaryKeyProperty = type.GetPrimaryKey()
				?? throw new InvalidOperationException($"Cannot delete objects of type {type.Name} without primary key");

			var primaryKeyValues = group
				.Select(primaryKeyProperty.GetValue)
				.Where(key => key is not null)
				.ToArray();

			if (primaryKeyValues.Length > 0)
			{
				await conn.DeleteAllIdsAsync(
					primaryKeyValues,
					type.GetTableName(),
					primaryKeyProperty.GetColumnName()
				);
			}
		}
	}

	static void RefreshForeignKeys(object element)
	{
		var type = element.GetType();
		foreach (var relationshipProperty in type.GetRelationshipProperties())
		{
			var relationshipAttribute = relationshipProperty.GetAttribute<RelationshipAttribute>();

			// Ignore read-only attributes
			if (relationshipAttribute.ReadOnly)
			{
				continue;
			}

			if (relationshipAttribute is OneToOneAttribute || relationshipAttribute is ManyToOneAttribute)
			{
				var foreignKeyProperty = type.GetForeignKeyProperty(relationshipProperty);
				if (foreignKeyProperty != null)
				{
					var entityType = relationshipProperty.GetEntityType(out EnclosedType enclosedType);
					var destinationPrimaryKeyProperty = entityType.GetPrimaryKey();
					Assert(enclosedType == EnclosedType.None, type, relationshipProperty, "ToOne relationships cannot be lists or arrays");
					Assert(destinationPrimaryKeyProperty != null, type, relationshipProperty, "Found foreign key but destination Type doesn't have primary key");

					var relationshipValue = relationshipProperty.GetValue(element, null);
					object foreignKeyValue = null;
					if (relationshipValue != null)
					{
						foreignKeyValue = destinationPrimaryKeyProperty.GetValue(relationshipValue, null);
					}
					foreignKeyProperty.SetValue(element, foreignKeyValue, null);
				}
			}
			else if (relationshipAttribute is TextBlobAttribute)
			{
				TextBlobOperations.UpdateTextBlobProperty(element, relationshipProperty);
			}
		}
	}


	static async Task UpdateInverseForeignKeysAsync(this SQLiteAsyncConnection conn, object element)
	{
		foreach (var relationshipProperty in element.GetType().GetRelationshipProperties())
		{
			var relationshipAttribute = relationshipProperty.GetAttribute<RelationshipAttribute>();

			// Ignore read-only attributes
			if (relationshipAttribute.ReadOnly)
			{
				continue;
			}

			if (relationshipAttribute is OneToManyAttribute)
			{
				await conn.UpdateOneToManyInverseForeignKeyAsync(element, relationshipProperty);
			}
			else if (relationshipAttribute is OneToOneAttribute)
			{
				await conn.UpdateOneToOneInverseForeignKey(element, relationshipProperty);
			}
			else if (relationshipAttribute is ManyToManyAttribute)
			{
				await conn.UpdateManyToManyForeignKeys(element, relationshipProperty);
			}
		}
	}

	static async Task UpdateOneToManyInverseForeignKeyAsync(this SQLiteAsyncConnection conn, object element, PropertyInfo relationshipProperty)
	{
		var type = element.GetType();

		var entityType = relationshipProperty.GetEntityType(out EnclosedType enclosedType);

		var originPrimaryKeyProperty = type.GetPrimaryKey();
		var inversePrimaryKeyProperty = entityType.GetPrimaryKey();
		var inverseForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty, inverse: true);

		Assert(enclosedType != EnclosedType.None, type, relationshipProperty, "OneToMany relationships must be List or Array of entities");
		Assert(originPrimaryKeyProperty != null, type, relationshipProperty, "OneToMany relationships require Primary Key in the origin entity");
		Assert(inversePrimaryKeyProperty != null, type, relationshipProperty, "OneToMany relationships require Primary Key in the destination entity");
		Assert(inverseForeignKeyProperty != null, type, relationshipProperty, "Unable to find foreign key for OneToMany relationship");

		var inverseProperty = type.GetInverseProperty(relationshipProperty);
		if (inverseProperty != null)
		{
			var inverseEntityType = inverseProperty.GetEntityType(out EnclosedType inverseEnclosedType);
			Assert(inverseEnclosedType == EnclosedType.None, type, relationshipProperty, "OneToMany inverse relationship shouldn't be List or Array");
			Assert(inverseEntityType == type, type, relationshipProperty, "OneToMany inverse relationship is not the expected type");
		}

		var keyValue = originPrimaryKeyProperty.GetValue(element, null);
		var children = (IEnumerable)relationshipProperty.GetValue(element, null);
		var childrenKeyList = new List<object>();
		if (children != null)
		{
			foreach (var child in children)
			{
				var childKey = inversePrimaryKeyProperty.GetValue(child, null);
				childrenKeyList.Add(childKey);

				inverseForeignKeyProperty.SetValue(child, keyValue, null);
				inverseProperty?.SetValue(child, element, null);
			}
		}


		// Delete previous relationships
		var deleteQuery = string.Format("update [{0}] set [{1}] = NULL where [{1}] == ?",
			entityType.GetTableName(), inverseForeignKeyProperty.GetColumnName());
		var deleteParamaters = new List<object> { keyValue };
		await conn.ExecuteAsync(deleteQuery, [.. deleteParamaters]);

		var chunks = Split(childrenKeyList, queryLimit);
		var loopTo = chunks.Count == 0 ? 1 : chunks.Count;
		for (int i = 0; i < loopTo; i++)
		{
			var chunk = chunks.Count > i ? chunks[i] : [];
			// Objects already updated, now change the database
			var childrenPlaceHolders = string.Join(",", Enumerable.Repeat("?", chunk.Count));
			var query = string.Format("update [{0}] set [{1}] = ? where [{2}] in ({3})",
				entityType.GetTableName(), inverseForeignKeyProperty.GetColumnName(), inversePrimaryKeyProperty.GetColumnName(), childrenPlaceHolders);

			var parameters = new List<object> { keyValue };
			parameters.AddRange(chunk);
			await conn.ExecuteAsync(query, [.. parameters]);
		}
	}

	static async Task UpdateOneToOneInverseForeignKey(this SQLiteAsyncConnection conn, object element, PropertyInfo relationshipProperty)
	{
		var type = element.GetType();

		var entityType = relationshipProperty.GetEntityType(out EnclosedType enclosedType);

		var originPrimaryKeyProperty = type.GetPrimaryKey();
		var inversePrimaryKeyProperty = entityType.GetPrimaryKey();
		var inverseForeignKeyProperty = type.GetForeignKeyProperty(relationshipProperty, inverse: true);

		Assert(enclosedType == EnclosedType.None, type, relationshipProperty, "OneToOne relationships cannot be List or Array of entities");

		var inverseProperty = type.GetInverseProperty(relationshipProperty);
		if (inverseProperty != null)
		{
			EnclosedType inverseEnclosedType;
			var inverseEntityType = inverseProperty.GetEntityType(out inverseEnclosedType);
			Assert(inverseEnclosedType == EnclosedType.None, type, relationshipProperty, "OneToOne inverse relationship shouldn't be List or Array");
			Assert(inverseEntityType == type, type, relationshipProperty, "OneToOne inverse relationship is not the expected type");
		}

		object keyValue = null;
		if (originPrimaryKeyProperty != null && inverseForeignKeyProperty != null)
		{
			keyValue = originPrimaryKeyProperty.GetValue(element, null);
		}

		object childKey = null;
		var child = relationshipProperty.GetValue(element, null);
		if (child != null)
		{
			if (inverseForeignKeyProperty != null && keyValue != null)
			{
				inverseForeignKeyProperty.SetValue(child, keyValue, null);
			}
			inverseProperty?.SetValue(child, element, null);
			if (inversePrimaryKeyProperty != null)
			{
				childKey = inversePrimaryKeyProperty.GetValue(child, null);
			}
		}


		// Objects already updated, now change the database
		if (inverseForeignKeyProperty != null && inversePrimaryKeyProperty != null)
		{
			var query = string.Format("update [{0}] set [{1}] = ? where [{2}] == ?",
				entityType.GetTableName(), inverseForeignKeyProperty.GetColumnName(), inversePrimaryKeyProperty.GetColumnName());
			await conn.ExecuteAsync(query, keyValue, childKey);

			// Delete previous relationships
			var deleteQuery = string.Format("update [{0}] set [{1}] = NULL where [{1}] == ? and [{2}] not in (?)",
				entityType.GetTableName(), inverseForeignKeyProperty.GetColumnName(), inversePrimaryKeyProperty.GetColumnName());
			await conn.ExecuteAsync(deleteQuery, keyValue, childKey ?? "");
		}
	}

	static async Task UpdateManyToManyForeignKeys(this SQLiteAsyncConnection conn, object element, PropertyInfo relationshipProperty)
	{
		var type = element.GetType();

		var entityType = relationshipProperty.GetEntityType(out EnclosedType enclosedType);

		var currentEntityPrimaryKeyProperty = type.GetPrimaryKey();
		var otherEntityPrimaryKeyProperty = entityType.GetPrimaryKey();
		var manyToManyMetaInfo = type.GetManyToManyMetaInfo(relationshipProperty);
		var currentEntityForeignKeyProperty = manyToManyMetaInfo.OriginProperty;
		var otherEntityForeignKeyProperty = manyToManyMetaInfo.DestinationProperty;
		var intermediateType = manyToManyMetaInfo.IntermediateType;

		Assert(enclosedType != EnclosedType.None, type, relationshipProperty, "ManyToMany relationship must be a List or Array");
		Assert(currentEntityPrimaryKeyProperty != null, type, relationshipProperty, "ManyToMany relationship origin must have Primary Key");
		Assert(otherEntityPrimaryKeyProperty != null, type, relationshipProperty, "ManyToMany relationship destination must have Primary Key");
		Assert(intermediateType != null, type, relationshipProperty, "ManyToMany relationship intermediate type cannot be null");
		Assert(currentEntityForeignKeyProperty != null, type, relationshipProperty, "ManyToMany relationship origin must have a foreign key defined in the intermediate type");
		Assert(otherEntityForeignKeyProperty != null, type, relationshipProperty, "ManyToMany relationship destination must have a foreign key defined in the intermediate type");

		var primaryKey = currentEntityPrimaryKeyProperty.GetValue(element, null);

		// Obtain the list of children keys
		var childList = (IEnumerable)relationshipProperty.GetValue(element, null);
		var childKeyList = (from object child in childList ?? new List<object>()
							select otherEntityPrimaryKeyProperty.GetValue(child, null)).ToList();

		List<object> currentChildKeyList = [];
		var chunks = Split(childKeyList, queryLimit);
		var loopTo = chunks.Count == 0 ? 1 : chunks.Count;
		for (int i = 0; i < loopTo; i++)
		{
			var chunk = chunks.Count > i ? chunks[i] : new List<object>();
			// Check for already existing relationships
			var childrenPlaceHolders = string.Join(",", Enumerable.Repeat("?", chunk.Count));
			var currentChildrenQuery = string.Format("select [{0}] from [{1}] where [{2}] == ? and [{0}] in ({3})",
				otherEntityForeignKeyProperty.GetColumnName(), intermediateType.GetTableName(), currentEntityForeignKeyProperty.GetColumnName(), childrenPlaceHolders);

			var parameters = new List<object> { primaryKey };
			parameters.AddRange(chunk);
			currentChildKeyList.AddRange(
				from object child in
					await conn.QueryAsync(await conn.GetMappingAsync(intermediateType), currentChildrenQuery, [.. parameters])
				select otherEntityForeignKeyProperty.GetValue(child, null));
		}

		// Insert missing relationships in the intermediate table
		var missingChildKeyList = childKeyList.Where(o => !currentChildKeyList.Contains(o)).ToList();
		var missingIntermediateObjects = new List<object>(missingChildKeyList.Count);
		foreach (var missingChildKey in missingChildKeyList)
		{
			var intermediateObject = Activator.CreateInstance(intermediateType);
			currentEntityForeignKeyProperty.SetValue(intermediateObject, primaryKey, null);
			otherEntityForeignKeyProperty.SetValue(intermediateObject, missingChildKey, null);

			missingIntermediateObjects.Add(intermediateObject);
		}

		await conn.InsertAllAsync(missingIntermediateObjects);

		for (int i = 0; i < loopTo; i++)
		{
			var chunk = chunks.Count > i ? chunks[i] : new List<object>();
			var childrenPlaceHolders = string.Join(",", Enumerable.Repeat("?", chunk.Count));

			// Delete any other pending relationship
			var deleteQuery = string.Format("delete from [{0}] where [{1}] == ? and [{2}] not in ({3})",
				intermediateType.GetTableName(), currentEntityForeignKeyProperty.GetColumnName(),
				otherEntityForeignKeyProperty.GetColumnName(), childrenPlaceHolders);

			var parameters = new List<object> { primaryKey };
			parameters.AddRange(chunk);
			await conn.ExecuteAsync(deleteQuery, [.. parameters]);
		}

	}

	static async Task DeleteAllIdsAsync(this SQLiteAsyncConnection conn, object[] primaryKeyValues, string entityName, string primaryKeyName)
	{
		if (primaryKeyValues == null || primaryKeyValues.Length == 0)
		{
			return;
		}

		if (primaryKeyValues.Length <= queryLimit)
		{
			var placeholdersString = string.Join(",", Enumerable.Repeat("?", primaryKeyValues.Length));
			var deleteQuery = string.Format("delete from [{0}] where [{1}] in ({2})", entityName, primaryKeyName, placeholdersString);

			await conn.ExecuteAsync(deleteQuery, primaryKeyValues);
		}
		else
		{
			foreach (var primaryKeys in Split(primaryKeyValues.ToList(), queryLimit))
			{
				await conn.DeleteAllIdsAsync([.. primaryKeys], entityName, primaryKeyName);
			}

		}
	}

	static List<List<T>> Split<T>(List<T> items, int sliceSize = 30)
	{
		ArgumentNullException.ThrowIfNull(items);
		ArgumentOutOfRangeException.ThrowIfLessThan(sliceSize, 1);

		var totalSlices = (int)Math.Ceiling(items.Count / (double)sliceSize);
		var result = new List<List<T>>(capacity: totalSlices);

		for (int i = 0; i < items.Count; i += sliceSize)
		{
			result.Add(items.GetRange(i, Math.Min(sliceSize, items.Count - i)));
		}

		return result;
	}

	static void Assert(bool assertion, Type type, PropertyInfo property, string message)
	{
		if (EnableRuntimeAssertions && !assertion)
		{
			throw new IncorrectRelationshipException(type.Name, property != null ? property.Name : string.Empty, message);
		}
	}
	#endregion
}
