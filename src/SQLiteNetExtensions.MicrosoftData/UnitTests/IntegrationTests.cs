using Microsoft.Data.Sqlite;
using SQLiteNetExtensions.MicrosoftData.Attributes;
using SQLiteNetExtensions.MicrosoftData.Extensions;
using SQLiteNetExtensions.MicrosoftData.MicroOrm;

namespace SQLiteNetExtensions.MicrosoftData.UnitTests;

#region Test Models

public class Person
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Name { get; set; }

	[OneToOne("PassportId")]
	public Passport? Passport { get; set; }
	public int PassportId { get; set; }

	[OneToMany]
	public List<Pet>? Pets { get; set; }
}

public class Passport
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Number { get; set; }

	[OneToOne]
	public Person? Owner { get; set; }
}

public class Pet
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Name { get; set; }

	[ForeignKey(typeof(Person))]
	public int OwnerId { get; set; }

	[ManyToOne]
	public Person? Owner { get; set; }
}

public class Student
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Name { get; set; }

	[ManyToMany(typeof(StudentCourse))]
	public List<Course>? Courses { get; set; }
}

public class Course
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Title { get; set; }

	[ManyToMany(typeof(StudentCourse))]
	public List<Student>? Students { get; set; }
}

public class StudentCourse
{
	[ForeignKey(typeof(Student))]
	public int StudentId { get; set; }

	[ForeignKey(typeof(Course))]
	public int CourseId { get; set; }
}

public class TypeTestEntity
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? StringVal { get; set; }
	public int IntVal { get; set; }
	public long LongVal { get; set; }
	public double DoubleVal { get; set; }
	public bool BoolVal { get; set; }
	public DateTime DateTimeVal { get; set; }
	public Guid GuidVal { get; set; }
	public byte[]? ByteArrayVal { get; set; }
	public TestEnum EnumVal { get; set; }
	public int? NullableIntVal { get; set; }
}

public enum TestEnum
{
	ValueA = 0,
	ValueB = 1,
	ValueC = 2
}

public class CascadeParent
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Name { get; set; }

	[OneToMany(inverseProperty: "Parent", CascadeOperations = CascadeOperation.All)]
	public List<CascadeChild>? Children { get; set; }
}

public class CascadeChild
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Name { get; set; }

	[ForeignKey(typeof(CascadeParent))]
	public int ParentId { get; set; }

	[ManyToOne]
	public CascadeParent? Parent { get; set; }
}

public class TextBlobEntity
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string? Name { get; set; }

	public string? TagsBlobbed { get; set; }

	[TextBlob("TagsBlobbed")]
	public List<string>? Tags { get; set; }
}

#endregion

[TestFixture]
public class IntegrationTests
{
	SqliteConnection CreateConnection()
	{
		var conn = new SqliteConnection("Data Source=:memory:");
		conn.Open();
		return conn;
	}

	#region MicroOrm CRUD Tests

	[Test]
	public void CreateTable_And_Insert_Get_Roundtrip()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var entity = new TypeTestEntity
		{
			StringVal = "Hello",
			IntVal = 42,
			LongVal = 123456789L,
			DoubleVal = 3.14,
			BoolVal = true,
			DateTimeVal = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
			GuidVal = Guid.NewGuid(),
			ByteArrayVal = [1, 2, 3, 4, 5],
			EnumVal = TestEnum.ValueB,
			NullableIntVal = 99
		};

		conn.Insert(entity);
		Assert.That(entity.Id, Is.GreaterThan(0), "AutoIncrement should set the Id");

		var retrieved = conn.Get<TypeTestEntity>(entity.Id);
		Assert.Multiple(() =>
		{
			Assert.That(retrieved.StringVal, Is.EqualTo("Hello"));
			Assert.That(retrieved.IntVal, Is.EqualTo(42));
			Assert.That(retrieved.LongVal, Is.EqualTo(123456789L));
			Assert.That(retrieved.DoubleVal, Is.EqualTo(3.14).Within(0.001));
			Assert.That(retrieved.BoolVal, Is.True);
			Assert.That(retrieved.GuidVal, Is.EqualTo(entity.GuidVal));
			Assert.That(retrieved.EnumVal, Is.EqualTo(TestEnum.ValueB));
			Assert.That(retrieved.NullableIntVal, Is.EqualTo(99));
		});
	}

	[Test]
	public void Update_Changes_Persisted()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var entity = new TypeTestEntity { StringVal = "Original", IntVal = 1 };
		conn.Insert(entity);

		entity.StringVal = "Updated";
		entity.IntVal = 2;
		conn.Update(entity);

		var retrieved = conn.Get<TypeTestEntity>(entity.Id);
		Assert.Multiple(() =>
		{
			Assert.That(retrieved.StringVal, Is.EqualTo("Updated"));
			Assert.That(retrieved.IntVal, Is.EqualTo(2));
		});
	}

	[Test]
	public void Delete_Removes_Entity()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var entity = new TypeTestEntity { StringVal = "ToDelete" };
		conn.Insert(entity);

		SqliteConnectionExtensions.Delete(conn, entity);

		var found = conn.Find<TypeTestEntity>(entity.Id);
		Assert.That(found, Is.Null);
	}

	[Test]
	public void InsertOrReplace_Upsert()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var entity = new TypeTestEntity { StringVal = "First" };
		conn.Insert(entity);

		entity.StringVal = "Replaced";
		conn.InsertOrReplace(entity);

		var all = conn.Table<TypeTestEntity>().ToList();
		Assert.That(all, Has.Count.EqualTo(1));
		Assert.That(all[0].StringVal, Is.EqualTo("Replaced"));
	}

	[Test]
	public void Find_Returns_Null_When_Not_Found()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var found = conn.Find<TypeTestEntity>(999);
		Assert.That(found, Is.Null);
	}

	[Test]
	public void Table_Where_Filter()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		conn.Insert(new TypeTestEntity { StringVal = "A", IntVal = 1 });
		conn.Insert(new TypeTestEntity { StringVal = "B", IntVal = 2 });
		conn.Insert(new TypeTestEntity { StringVal = "C", IntVal = 3 });

		var results = conn.Table<TypeTestEntity>().Where(e => e.IntVal > 1).ToList();
		Assert.That(results, Has.Count.EqualTo(2));
	}

	[Test]
	public void DeleteAllIds_Batch_Delete()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var entities = Enumerable.Range(1, 5)
			.Select(i => new TypeTestEntity { StringVal = $"Entity{i}" })
			.ToList();

		foreach (var e in entities)
		{
			conn.Insert(e);
		}

		var idsToDelete = entities.Take(3).Select(e => (object)e.Id).ToArray();
		SqliteConnectionExtensions.DeleteAllIds(conn, idsToDelete, "TypeTestEntity", "Id");

		var remaining = conn.Table<TypeTestEntity>().ToList();
		Assert.That(remaining, Has.Count.EqualTo(2));
	}

	#endregion

	#region OneToOne Tests

	[Test]
	public void OneToOne_InsertAndRead()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Person>();
		conn.CreateTable<Passport>();
		conn.CreateTable<Pet>();

		var passport = new Passport { Number = "AB123456" };
		conn.Insert(passport);

		var person = new Person { Name = "John", Passport = passport };
		conn.InsertWithChildren(person);

		var fetched = conn.GetWithChildren<Person>(person.Id);
		Assert.That(fetched.Passport, Is.Not.Null);
		Assert.That(fetched.Passport!.Number, Is.EqualTo("AB123456"));
	}

	#endregion

	#region OneToMany Tests

	[Test]
	public void OneToMany_InsertAndRead()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Person>();
		conn.CreateTable<Passport>();
		conn.CreateTable<Pet>();

		var person = new Person { Name = "Jane" };
		conn.Insert(person);

		var pets = new List<Pet>
		{
			new() { Name = "Buddy" },
			new() { Name = "Max" }
		};

		foreach (var pet in pets)
		{
			conn.Insert(pet);
		}

		person.Pets = pets;
		conn.UpdateWithChildren(person);

		var fetched = conn.GetWithChildren<Person>(person.Id);
		Assert.That(fetched.Pets, Is.Not.Null);
		Assert.That(fetched.Pets!, Has.Count.EqualTo(2));
	}

	#endregion

	#region ManyToOne Tests

	[Test]
	public void ManyToOne_InsertAndRead()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Person>();
		conn.CreateTable<Pet>();

		var person = new Person { Name = "Bob" };
		conn.Insert(person);

		var pet = new Pet { Name = "Rex", OwnerId = person.Id };
		conn.Insert(pet);

		var fetched = conn.GetWithChildren<Pet>(pet.Id);
		Assert.That(fetched.Owner, Is.Not.Null);
		Assert.That(fetched.Owner!.Name, Is.EqualTo("Bob"));
	}

	#endregion

	#region ManyToMany Tests

	[Test]
	public void ManyToMany_InsertAndRead()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Student>();
		conn.CreateTable<Course>();
		conn.CreateTable<StudentCourse>();

		var student = new Student { Name = "Alice" };
		conn.Insert(student);

		var course1 = new Course { Title = "Math" };
		var course2 = new Course { Title = "Science" };
		conn.Insert(course1);
		conn.Insert(course2);

		student.Courses = [course1, course2];
		conn.UpdateWithChildren(student);

		var fetchedStudent = conn.GetWithChildren<Student>(student.Id);
		Assert.That(fetchedStudent.Courses, Is.Not.Null);
		Assert.That(fetchedStudent.Courses!, Has.Count.EqualTo(2));

		// Read inverse
		var fetchedCourse = conn.GetWithChildren<Course>(course1.Id);
		Assert.That(fetchedCourse.Students, Is.Not.Null);
		Assert.That(fetchedCourse.Students!, Has.Count.EqualTo(1));
		Assert.That(fetchedCourse.Students![0].Name, Is.EqualTo("Alice"));
	}

	#endregion

	#region Cascade Tests

	[Test]
	public void CascadeInsert_RecursiveChildren()
	{
		using var conn = CreateConnection();
		conn.CreateTable<CascadeParent>();
		conn.CreateTable<CascadeChild>();

		var parent = new CascadeParent
		{
			Name = "Parent",
			Children =
			[
				new CascadeChild { Name = "Child1" },
				new CascadeChild { Name = "Child2" }
			]
		};

		conn.InsertWithChildren(parent, recursive: true);

		Assert.That(parent.Id, Is.GreaterThan(0));
		Assert.That(parent.Children![0].Id, Is.GreaterThan(0));
		Assert.That(parent.Children![1].Id, Is.GreaterThan(0));

		var fetched = conn.GetWithChildren<CascadeParent>(parent.Id, recursive: true);
		Assert.That(fetched.Children, Has.Count.EqualTo(2));
	}

	[Test]
	public void CascadeDelete_RecursiveChildren()
	{
		using var conn = CreateConnection();
		conn.CreateTable<CascadeParent>();
		conn.CreateTable<CascadeChild>();

		var parent = new CascadeParent
		{
			Name = "Parent",
			Children =
			[
				new CascadeChild { Name = "Child1" },
				new CascadeChild { Name = "Child2" }
			]
		};

		conn.InsertWithChildren(parent, recursive: true);
		var child1Id = parent.Children![0].Id;
		var child2Id = parent.Children![1].Id;

		// Load children first (cascade delete needs them loaded)
		conn.GetChildren(parent, recursive: true);
		conn.Delete(parent, recursive: true);

		var foundParent = conn.Find<CascadeParent>(parent.Id);
		var foundChild1 = conn.Find<CascadeChild>(child1Id);
		var foundChild2 = conn.Find<CascadeChild>(child2Id);

		Assert.Multiple(() =>
		{
			Assert.That(foundParent, Is.Null);
			Assert.That(foundChild1, Is.Null);
			Assert.That(foundChild2, Is.Null);
		});
	}

	#endregion

	#region TextBlob Tests

	[Test]
	public void TextBlob_SerializeAndDeserialize()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TextBlobEntity>();

		var entity = new TextBlobEntity
		{
			Name = "Test",
			Tags = ["tag1", "tag2", "tag3"]
		};

		conn.InsertWithChildren(entity);

		// Verify the blob was stored
		var raw = conn.Get<TextBlobEntity>(entity.Id);
		Assert.That(raw.TagsBlobbed, Is.Not.Null.And.Not.Empty);

		// Load with children should deserialize the blob
		var fetched = conn.GetWithChildren<TextBlobEntity>(entity.Id);
		Assert.That(fetched.Tags, Is.Not.Null);
		Assert.That(fetched.Tags!, Has.Count.EqualTo(3));
		Assert.That(fetched.Tags!, Does.Contain("tag1"));
		Assert.That(fetched.Tags!, Does.Contain("tag2"));
		Assert.That(fetched.Tags!, Does.Contain("tag3"));
	}

	#endregion

	#region GetAllWithChildren Tests

	[Test]
	public void GetAllWithChildren_WithFilter()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Person>();
		conn.CreateTable<Passport>();
		conn.CreateTable<Pet>();

		var person1 = new Person { Name = "Alice" };
		var person2 = new Person { Name = "Bob" };
		conn.Insert(person1);
		conn.Insert(person2);

		var pet1 = new Pet { Name = "Fido", OwnerId = person1.Id };
		conn.Insert(pet1);
		person1.Pets = [pet1];
		conn.UpdateWithChildren(person1);

		var results = conn.GetAllWithChildren<Person>(p => p.Name == "Alice");
		Assert.That(results, Has.Count.EqualTo(1));
		Assert.That(results[0].Pets, Is.Not.Null);
		Assert.That(results[0].Pets!, Has.Count.EqualTo(1));
	}

	#endregion

	#region Async Tests

	[Test]
	public async Task GetWithChildrenAsync_Works()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Person>();
		conn.CreateTable<Passport>();
		conn.CreateTable<Pet>();

		var person = new Person { Name = "AsyncPerson" };
		conn.Insert(person);

		var pet = new Pet { Name = "AsyncPet" };
		conn.Insert(pet);
		person.Pets = [pet];
		conn.UpdateWithChildren(person);

		var fetched = await conn.GetWithChildrenAsync<Person>(person.Id);
		Assert.That(fetched.Pets, Is.Not.Null);
		Assert.That(fetched.Pets!, Has.Count.EqualTo(1));
		Assert.That(fetched.Pets![0].Name, Is.EqualTo("AsyncPet"));
	}

	[Test]
	public async Task InsertWithChildrenAsync_CascadeInsert()
	{
		using var conn = CreateConnection();
		conn.CreateTable<CascadeParent>();
		conn.CreateTable<CascadeChild>();

		var parent = new CascadeParent
		{
			Name = "AsyncParent",
			Children =
			[
				new CascadeChild { Name = "AsyncChild1" },
				new CascadeChild { Name = "AsyncChild2" }
			]
		};

		await conn.InsertWithChildrenAsync(parent, recursive: true);

		Assert.That(parent.Id, Is.GreaterThan(0));
		Assert.That(parent.Children![0].Id, Is.GreaterThan(0));

		var fetched = await conn.GetWithChildrenAsync<CascadeParent>(parent.Id, recursive: true);
		Assert.That(fetched.Children, Has.Count.EqualTo(2));
	}

	[Test]
	public async Task GetAllWithChildrenAsync_WithFilter()
	{
		using var conn = CreateConnection();
		conn.CreateTable<Person>();
		conn.CreateTable<Passport>();
		conn.CreateTable<Pet>();

		var person1 = new Person { Name = "FilterAlice" };
		var person2 = new Person { Name = "FilterBob" };
		conn.Insert(person1);
		conn.Insert(person2);

		var pet1 = new Pet { Name = "FilterPet", OwnerId = person1.Id };
		conn.Insert(pet1);
		person1.Pets = [pet1];
		conn.UpdateWithChildren(person1);

		var results = await conn.GetAllWithChildrenAsync<Person>(p => p.Name == "FilterAlice");
		Assert.That(results, Has.Count.EqualTo(1));
		Assert.That(results[0].Pets, Is.Not.Null);
		Assert.That(results[0].Pets!, Has.Count.EqualTo(1));
	}

	[Test]
	public async Task DeleteAsync_CascadeDelete()
	{
		using var conn = CreateConnection();
		conn.CreateTable<CascadeParent>();
		conn.CreateTable<CascadeChild>();

		var parent = new CascadeParent
		{
			Name = "DeleteParent",
			Children =
			[
				new CascadeChild { Name = "DeleteChild" }
			]
		};

		await conn.InsertWithChildrenAsync(parent, recursive: true);
		var childId = parent.Children![0].Id;

		await conn.GetChildrenAsync(parent, recursive: true);
		await conn.DeleteAsync(parent, recursive: true);

		var foundParent = conn.Find<CascadeParent>(parent.Id);
		var foundChild = conn.Find<CascadeChild>(childId);

		Assert.Multiple(() =>
		{
			Assert.That(foundParent, Is.Null);
			Assert.That(foundChild, Is.Null);
		});
	}

	#endregion

	#region InsertAll Tests

	[Test]
	public void InsertAll_BatchInsert()
	{
		using var conn = CreateConnection();
		conn.CreateTable<TypeTestEntity>();

		var entities = Enumerable.Range(1, 10)
			.Select(i => new TypeTestEntity { StringVal = $"Entity{i}", IntVal = i })
			.ToList();

		SqliteConnectionExtensions.InsertAll(conn, entities);

		var all = conn.Table<TypeTestEntity>().ToList();
		Assert.That(all, Has.Count.EqualTo(10));
	}

	#endregion
}
