using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using IntegratedTestsSampleApp.Helpers;

namespace IntegratedTestsSampleApp.Tests;

public static class OneToOneTestsAsync
{

    #region classes
    public class O2OClassA
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(O2OClassB))]     // Explicit foreign key attribute
        public int OneClassBKey { get; set; }

        [OneToOne]
        public O2OClassB OneClassB { get; set; }
    }

    public class O2OClassB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    public class O2OClassC
    {
        [PrimaryKey, AutoIncrement]
        public int ClassId { get; set; }

        [OneToOne]     // OneToOne Foreign key can be declared in the referenced class
        public O2OClassD ElementD { get; set; }

        public string Bar { get; set; }
    }

    public class O2OClassD
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(O2OClassC))]    // Explicit foreign key attribute for a inverse relationship
        public int ObjectCKey { get; set; }

        public string Foo { get; set; }
    }

    public class O2OClassE
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ObjectFKey { get; set; }

        [OneToOne("ObjectFKey")]        // Explicit foreign key declaration
        public O2OClassF ObjectF { get; set; }

        public string Foo { get; set; }
    }

    public class O2OClassF
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [OneToOne]      // Inverse relationship, doesn't need foreign key
        public O2OClassE ObjectE { get; set; }

        public string Bar { get; set; }
    }
    #endregion

    public static async Task<Tuple<bool, string>> TestGetOneToOneDirectAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassA>();
            await conn.DropTableAsync<O2OClassB>();
            await conn.CreateTableAsync<O2OClassA>();
            await conn.CreateTableAsync<O2OClassB>();

            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectB);

            var objectA = new O2OClassA();
            await conn.InsertAsync(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetOneToOneDirectAsync: Failed at checking OneClassB is null");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetOneToOneDirectAsync: Failed at fetching relationship");

            // Set the relationship using IDs
            objectA.OneClassBKey = objectB.Id;
            await conn.UpdateAsync(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetOneToOneDirectAsync: Failed at updating relationship");

            // Fetch the relationship
            await conn.GetChildrenAsync(objectA);

            if (objectA.OneClassB == null)
                return new Tuple<bool, string>(false, "TestGetOneToOneDirectAsync: Failed at fetching the relationship after update");

            if (objectB.Id != objectA.OneClassB.Id || objectB.Foo != objectA.OneClassB.Foo)
                return new Tuple<bool, string>(false, "TestGetOneToOneDirectAsync: Relationship data mismatch");

            return new Tuple<bool, string>(true, "TestGetOneToOneDirectAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToOneDirectAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetOneToOneInverseForeignKeyAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassC>();
            await conn.DropTableAsync<O2OClassD>();
            await conn.CreateTableAsync<O2OClassC>();
            await conn.CreateTableAsync<O2OClassD>();

            // Use standard SQLite-Net API to create a new relationship
            var objectC = new O2OClassC
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectC);

            if (objectC.ElementD != null)
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKeyAsync: Failed at checking ElementD is null");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectC);

            if (objectC.ElementD != null)
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKeyAsync: Failed at fetching relationship");

            var objectD = new O2OClassD
            {
                ObjectCKey = objectC.ClassId,
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectD);

            // Fetch the relationship
            await conn.GetChildrenAsync(objectC);

            if (objectC.ElementD == null)
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKeyAsync: Failed at fetching relationship after insert");

            if (objectC.ClassId != objectC.ElementD.ObjectCKey || objectD.Foo != objectC.ElementD.Foo)
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKeyAsync: Relationship data mismatch");

            return new Tuple<bool, string>(true, "TestGetOneToOneInverseForeignKeyAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToOneInverseForeignKeyAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetOneToOneWithInverseRelationshipAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassE>();
            await conn.DropTableAsync<O2OClassF>();
            await conn.CreateTableAsync<O2OClassE>();
            await conn.CreateTableAsync<O2OClassF>();

            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectF);

            var objectE = new O2OClassE
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectE);

            if (objectE.ObjectF != null)
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: objectE.ObjectF is not null before relationship set");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectE);

            if (objectE.ObjectF != null)
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: objectE.ObjectF is not null after GetChildren");

            objectE.ObjectFKey = objectF.Id;
            await conn.UpdateAsync(objectE);

            if (objectE.ObjectF != null)
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: objectE.ObjectF is still not null after update");

            // Fetch the relationship
            await conn.GetChildrenAsync(objectE);

            if (objectE.ObjectF == null)
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: objectE.ObjectF is null after GetChildren");

            if (objectF.Id != objectE.ObjectF.Id || objectF.Bar != objectE.ObjectF.Bar)
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: Relationship data mismatch");

            if (objectE.ObjectF.ObjectE == null || objectE.ObjectF.ObjectE.Foo != objectE.Foo)
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: Inverse relationship not correct");

            if (!ReferenceEquals(objectE, objectE.ObjectF.ObjectE))
                return Tuple.Create(false, "TestGetOneToOneWithInverseRelationshipAsync: Inverse relationship is not the same object");

            return Tuple.Create(true, "TestGetOneToOneWithInverseRelationshipAsync: Passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestGetOneToOneWithInverseRelationshipAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetInverseOneToOneRelationshipWithExplicitKeyAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassE>();
            await conn.DropTableAsync<O2OClassF>();
            await conn.CreateTableAsync<O2OClassE>();
            await conn.CreateTableAsync<O2OClassF>();

            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectF);

            var objectE = new O2OClassE
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectE);

            if (objectF.ObjectE != null)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: objectF.ObjectE is not null before relationship set");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectF);

            if (objectF.ObjectE != null)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: objectF.ObjectE is not null after GetChildren");

            // Set the relationship using IDs
            objectE.ObjectFKey = objectF.Id;
            await conn.UpdateAsync(objectE);

            if (objectF.ObjectE != null)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: objectF.ObjectE is still not null after update");

            // Fetch the relationship
            await conn.GetChildrenAsync(objectF);

            if (objectF.ObjectE == null)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: objectF.ObjectE is null after GetChildren");

            if (objectE.Foo != objectF.ObjectE.Foo)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: objectE.Foo is not equal objectF.ObjectE.Foo after GetChildren");

            // Check the inverse relationship
            if (objectF.ObjectE.ObjectF == null)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: objectF.ObjectE.ObjectF isn't null");

            if (objectE.Id != objectF.ObjectE.ObjectFKey || objectF.Bar != objectF.ObjectE.ObjectF.Bar)
                return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: Relationship data mismatch");

            return Tuple.Create(true, "TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: Passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestGetInverseOneToOneRelationshipWithExplicitKeyAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToOneRelationshipAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassA>();
            await conn.DropTableAsync<O2OClassB>();
            await conn.CreateTableAsync<O2OClassA>();
            await conn.CreateTableAsync<O2OClassB>();

            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectB);

            var objectA = new O2OClassA();
            await conn.InsertAsync(objectA);

            // Set the relationship using objects
            objectA.OneClassB = objectB;
            if (objectA.OneClassBKey != 0)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipAsync failed: OneClassBKey should be 0 before update");

            await conn.UpdateWithChildrenAsync(objectA);

            if (objectA.OneClassBKey != objectB.Id)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipAsync failed: Foreign key should have been refreshed");

            // Fetch the relationship
            var newObjectA = await conn.GetAsync<O2OClassA>(objectA.Id);

            if (newObjectA.OneClassBKey != objectB.Id)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipAsync failed: Foreign key in database is not updated");

            return Tuple.Create(true, "TestUpdateSetOneToOneRelationshipAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestUpdateSetOneToOneRelationshipAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateUnsetOneToOneRelationshipAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassA>();
            await conn.DropTableAsync<O2OClassB>();
            await conn.CreateTableAsync<O2OClassA>();
            await conn.CreateTableAsync<O2OClassB>();

            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectB);

            var objectA = new O2OClassA();
            await conn.InsertAsync(objectA);

            // Set the relationship using objects
            objectA.OneClassB = objectB;

            if (objectA.OneClassBKey != 0)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipAsync failed: Foreign key !=0");

            await conn.UpdateWithChildrenAsync(objectA);

            if (objectA.OneClassBKey != objectB.Id)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipAsync failed: Foreign key should have been refreshed");

            // Until here, test is same that TestUpdateSetOneToOneRelationship
            objectA.OneClassB = null; // Unset relationship

            if (objectA.OneClassBKey != objectB.Id)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipAsync failed: Foreign key shouldn't have been refreshed yet");

            await conn.UpdateWithChildrenAsync(objectA);

            if (objectA.OneClassBKey != 0)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipAsync failed: Foreign key hasn't been unset");

            return Tuple.Create(true, "TestUpdateUnsetOneToOneRelationshipAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestUpdateUnsetOneToOneRelationshipAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToOneRelationshipWithInverseAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassE>();
            await conn.DropTableAsync<O2OClassF>();
            await conn.CreateTableAsync<O2OClassE>();
            await conn.CreateTableAsync<O2OClassF>();

            var objectF = new O2OClassF
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectF);

            var objectE = new O2OClassE();
            await conn.InsertAsync(objectE);

            objectE.ObjectF = objectF;
            if (objectE.ObjectFKey != 0)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseAsync failed: ObjectFKey should be 0 before update");

            await conn.UpdateWithChildrenAsync(objectE);

            if (objectE.ObjectFKey != objectF.Id)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseAsync failed: Foreign key should have been refreshed");

            if (!ReferenceEquals(objectF, objectE.ObjectF))
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseAsync failed: Inverse relationship hasn't been set");

            var newObjectE = await conn.GetAsync<O2OClassE>(objectE.Id);

            if (newObjectE.ObjectFKey != objectF.Id)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseAsync failed: Foreign key in database is not updated");

            return Tuple.Create(true, "TestUpdateSetOneToOneRelationshipWithInverseAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestUpdateSetOneToOneRelationshipWithInverseAsync failed: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassF>();
            await conn.DropTableAsync<O2OClassE>();
            await conn.CreateTableAsync<O2OClassF>();
            await conn.CreateTableAsync<O2OClassE>();

            var objectF = new O2OClassF
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectF);

            var objectE = new O2OClassE();
            await conn.InsertAsync(objectE);

            objectF.ObjectE = objectE;
            if (objectE.ObjectFKey != 0)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync failed: ObjectFKey should be 0 before update");

            await conn.UpdateWithChildrenAsync(objectF);

            if (objectE.ObjectFKey != objectF.Id)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync failed: Foreign key should have been refreshed");

            if (!ReferenceEquals(objectF, objectE.ObjectF))
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync failed: Inverse relationship hasn't been set");

            var newObjectE = await conn.GetAsync<O2OClassE>(objectE.Id);

            if (newObjectE.ObjectFKey != objectF.Id)
                return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync failed: Foreign key in database is not updated");

            return Tuple.Create(true, "TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestUpdateSetOneToOneRelationshipWithInverseForeignKeyAsync failed: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassF>();
            await conn.DropTableAsync<O2OClassE>();
            await conn.CreateTableAsync<O2OClassF>();
            await conn.CreateTableAsync<O2OClassE>();

            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectF);

            var objectE = new O2OClassE();
            await conn.InsertAsync(objectE);

            // Set the relationship using objects
            objectF.ObjectE = objectE;
            if (objectE.ObjectFKey != 0)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync failed: Foreign key !=0");

            await conn.UpdateWithChildrenAsync(objectF);

            if (objectE.ObjectFKey != objectF.Id)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync failed: Foreign key should have been refreshed");

            // At this point the test is the same as TestUpdateSetOneToOneRelationshipWithInverseForeignKey
            objectF.ObjectE = null;     // Unset the relationship

            await conn.UpdateWithChildrenAsync(objectF);
            // Fetch the relationship
            var newObjectA = await conn.GetAsync<O2OClassE>(objectE.Id);
            if (newObjectA.ObjectFKey != 0)
                return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync failed: Foreign key should have been refreshed in database");

            return Tuple.Create(true, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestUpdateUnsetOneToOneRelationshipWithInverseForeignKeyAsync failed: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetAllNoFilterAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassA>();
            await conn.DropTableAsync<O2OClassB>();
            await conn.CreateTableAsync<O2OClassA>();
            await conn.CreateTableAsync<O2OClassB>();

            var a1 = new O2OClassA();
            var a2 = new O2OClassA();
            var a3 = new O2OClassA();
            var aObjects = new[] { a1, a2, a3 };
            await conn.InsertAllAsync(aObjects);

            var b1 = new O2OClassB { Foo = "Foo 1" };
            var b2 = new O2OClassB { Foo = "Foo 2" };
            var b3 = new O2OClassB { Foo = "Foo 3" };
            var bObjects = new[] { b1, b2, b3 };
            await conn.InsertAllAsync(bObjects);

            a1.OneClassB = b1;
            a2.OneClassB = b2;
            a3.OneClassB = b3;
            await conn.UpdateWithChildrenAsync(a1);
            await conn.UpdateWithChildrenAsync(a2);
            await conn.UpdateWithChildrenAsync(a3);

            var aElements = (await conn.GetAllWithChildrenAsync<O2OClassA>()).OrderBy(a => a.Id).ToArray();

            if (aObjects.Length != aElements.Length)
                return Tuple.Create(false, "TestGetAllNoFilterAsync failed: Number of elements does not match");

            for (int i = 0; i < aObjects.Length; i++)
            {
                if (aObjects[i].Id != aElements[i].Id)
                    return Tuple.Create(false, $"TestGetAllNoFilterAsync failed: Id mismatch at index {i}");
                if (aObjects[i].OneClassB.Id != aElements[i].OneClassB.Id)
                    return Tuple.Create(false, $"TestGetAllNoFilterAsync failed: OneClassB Id mismatch at index {i}");
                if (aObjects[i].OneClassB.Foo != aElements[i].OneClassB.Foo)
                    return Tuple.Create(false, $"TestGetAllNoFilterAsync failed: OneClassB Foo mismatch at index {i}");
            }

            return Tuple.Create(true, "TestGetAllNoFilterAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestGetAllNoFilterAsync failed: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetAllFilterAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2OClassC>();
            await conn.DropTableAsync<O2OClassD>();
            await conn.CreateTableAsync<O2OClassC>();
            await conn.CreateTableAsync<O2OClassD>();

            var c1 = new O2OClassC { Bar = "Bar 1" };
            var c2 = new O2OClassC { Bar = "Foo 2" };
            var c3 = new O2OClassC { Bar = "Bar 3" };
            var cObjects = new[] { c1, c2, c3 };
            await conn.InsertAllAsync(cObjects);

            var d1 = new O2OClassD { Foo = "Foo 1" };
            var d2 = new O2OClassD { Foo = "Foo 2" };
            var d3 = new O2OClassD { Foo = "Foo 3" };
            var bObjects = new[] { d1, d2, d3 };
            await conn.InsertAllAsync(bObjects);

            c1.ElementD = d1;
            c2.ElementD = d2;
            c3.ElementD = d3;
            await conn.UpdateWithChildrenAsync(c1);
            await conn.UpdateWithChildrenAsync(c2);
            await conn.UpdateWithChildrenAsync(c3);

            var expectedCObjects = cObjects.Where(c => c.Bar.Contains("Bar")).ToArray();
            var cElements = (await conn.GetAllWithChildrenAsync<O2OClassC>(c => c.Bar.Contains("Bar")))
                .OrderBy(a => a.ClassId).ToArray();

            if (expectedCObjects.Length != cElements.Length)
                return Tuple.Create(false, "TestGetAllFilterAsync failed: Number of elements does not match");

            for (int i = 0; i < expectedCObjects.Length; i++)
            {
                if (expectedCObjects[i].ClassId != cElements[i].ClassId)
                    return Tuple.Create(false, $"TestGetAllFilterAsync failed: ClassId mismatch at index {i}");
                if (expectedCObjects[i].ElementD.Id != cElements[i].ElementD.Id)
                    return Tuple.Create(false, $"TestGetAllFilterAsync failed: ElementD Id mismatch at index {i}");
                if (expectedCObjects[i].ElementD.Foo != cElements[i].ElementD.Foo)
                    return Tuple.Create(false, $"TestGetAllFilterAsync failed: ElementD Foo mismatch at index {i}");
            }

            return Tuple.Create(true, "TestGetAllFilterAsync passed");
        }
        catch (Exception ex)
        {
            return Tuple.Create(false, $"TestGetAllFilterAsync failed: {ex.Message}");
        }
    }
}
