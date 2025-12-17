using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using IntegratedTestsSampleApp.Helpers;

namespace IntegratedTestsSampleApp.Tests;

public static class OneToOneTests
{

    #region classes
    public class O2OClassA
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(O2OClassB))]     // Explicit foreign key attribute
        public int OneClassBKey { get; set; }

        [OneToOne]
        public O2OClassB? OneClassB { get; set; }
    }

    public class O2OClassB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; } = string.Empty;
    }

    public class O2OClassC
    {
        [PrimaryKey, AutoIncrement]
        public int ClassId { get; set; }

        [OneToOne]     // OneToOne Foreign key can be declared in the referenced class
        public O2OClassD? ElementD { get; set; }

        public string Bar { get; set; } = string.Empty;
    }

    public class O2OClassD
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(O2OClassC))]    // Explicit foreign key attribute for a inverse relationship
        public int ObjectCKey { get; set; }

        public string Foo { get; set; } = string.Empty;
    }

    public class O2OClassE
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ObjectFKey { get; set; }

        [OneToOne("ObjectFKey")]        // Explicit foreign key declaration
        public O2OClassF? ObjectF { get; set; }

        public string Foo { get; set; } = string.Empty;
    }

    public class O2OClassF
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [OneToOne]      // Inverse relationship, doesn't need foreign key
        public O2OClassE? ObjectE { get; set; }

        public string Bar { get; set; } = string.Empty;
    }
    #endregion

    public static Tuple<bool, string> TestGetOneToOneDirect()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2OClassA>();
            conn.DropTable<O2OClassB>();
            conn.CreateTable<O2OClassA>();
            conn.CreateTable<O2OClassB>();

            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            conn.Insert(objectB);

            var objectA = new O2OClassA();
            conn.Insert(objectA);

            if (objectA.OneClassB != null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneDirect: Failed at checking OneClassB is null");
            }

            // Fetch (yet empty) the relationship
            conn.GetChildren(objectA);

            if (objectA.OneClassB != null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneDirect: Failed at fetching relationship");
            }

            // Set the relationship using IDs
            objectA.OneClassBKey = objectB.Id;
            conn.Update(objectA);

            if (objectA.OneClassB != null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneDirect: Failed at updating relationship");
            }

            // Fetch the relationship
            conn.GetChildren(objectA);

            if (objectA.OneClassB == null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneDirect: Failed at fetching the relationship after update");
            }

            if (objectB.Id != objectA.OneClassB.Id || objectB.Foo != objectA.OneClassB.Foo)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneDirect: Relationship data mismatch");
            }

            return new Tuple<bool, string>(true, "TestGetOneToOneDirect: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestGetOneToOneDirect: Exception occurred");
        }
    }

    public static Tuple<bool, string> TestGetOneToOneInverseForeignKey()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2OClassC>();
            conn.DropTable<O2OClassD>();
            conn.CreateTable<O2OClassC>();
            conn.CreateTable<O2OClassD>();

            // Use standard SQLite-Net API to create a new relationship
            var objectC = new O2OClassC
            {
                Bar = string.Format("Bar String {0}", new Random().Next(100))
            };
            conn.Insert(objectC);

            if (objectC.ElementD != null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKey: Failed at checking ElementD is null");
            }

            // Fetch (yet empty) the relationship
            conn.GetChildren(objectC);

            if (objectC.ElementD != null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKey: Failed at fetching relationship");
            }

            var objectD = new O2OClassD
            {
                ObjectCKey = objectC.ClassId,
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            conn.Insert(objectD);

            // Fetch the relationship
            conn.GetChildren(objectC);

            if (objectC.ElementD == null)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKey: Failed at fetching relationship after insert");
            }

            if (objectC.ClassId != objectC.ElementD.ObjectCKey || objectD.Foo != objectC.ElementD.Foo)
            {
                return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKey: Relationship data mismatch");
            }

            return new Tuple<bool, string>(true, "TestGetOneToOneInverseForeignKey: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestGetOneToOneInverseForeignKey: Exception occurred");
        }
    }

    public static Tuple<bool, string> TestGetOneToOneWithInverseRelationship()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassE>();
        conn.DropTable<O2OClassF>();
        conn.CreateTable<O2OClassE>();
        conn.CreateTable<O2OClassF>();

        // Use standard SQLite-Net API to create a new relationship
        var objectF = new O2OClassF
        {
            Bar = string.Format("Bar String {0}", new Random().Next(100))
        };
        conn.Insert(objectF);

        var objectE = new O2OClassE
        {
            Foo = string.Format("Foo String {0}", new Random().Next(100))
        };
        conn.Insert(objectE);

        if (objectE.ObjectF != null)
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: objectE.ObjectF is not null before relationship set");
        }

        // Fetch (yet empty) the relationship
        conn.GetChildren(objectE);

        if (objectE.ObjectF != null)
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: objectE.ObjectF is not null after GetChildren");
        }

        objectE.ObjectFKey = objectF.Id;
        conn.Update(objectE);

        if (objectE.ObjectF != null)
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: objectE.ObjectF is still not null after update");
        }

        // Fetch the relationship
        conn.GetChildren(objectE);

        if (objectE.ObjectF == null)
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: objectE.ObjectF is null after GetChildren");
        }

        if (objectF.Id != objectE.ObjectF.Id || objectF.Bar != objectE.ObjectF.Bar)
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: Relationship data mismatch");
        }

        if (objectE.ObjectF.ObjectE == null || objectE.ObjectF.ObjectE.Foo != objectE.Foo)
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: Inverse relationship not correct");
        }

        if (!ReferenceEquals(objectE, objectE.ObjectF.ObjectE))
        {
            return Tuple.Create(false, "TestGetOneToOneWithInverseRelationship: Inverse relationship is not the same object");
        }

        return Tuple.Create(true, "TestGetOneToOneWithInverseRelationship: Passed");
    }

    public static Tuple<bool, string> TestGetInverseOneToOneRelationshipWithExplicitKey()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassE>();
        conn.DropTable<O2OClassF>();
        conn.CreateTable<O2OClassE>();
        conn.CreateTable<O2OClassF>();

        // Use standard SQLite-Net API to create a new relationship
        var objectF = new O2OClassF
        {
            Bar = string.Format("Bar String {0}", new Random().Next(100))
        };
        conn.Insert(objectF);

        var objectE = new O2OClassE
        {
            Foo = string.Format("Foo String {0}", new Random().Next(100))
        };
        conn.Insert(objectE);

        if (objectF.ObjectE != null)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: objectF.ObjectE is not null before relationship set");
        }

        // Fetch (yet empty) the relationship
        conn.GetChildren(objectF);

        if (objectF.ObjectE != null)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: objectF.ObjectE is not null after GetChildren");
        }

        // Set the relationship using IDs
        objectE.ObjectFKey = objectF.Id;
        conn.Update(objectE);

        if (objectF.ObjectE != null)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: objectF.ObjectE is still not null after update");
        }

        // Fetch the relationship
        conn.GetChildren(objectF);

        if (objectF.ObjectE == null)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: objectF.ObjectE is null after GetChildren");
        }

        if (objectE.Foo != objectF.ObjectE.Foo)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: objectE.Foo is not equal objectF.ObjectE.Foo after GetChildren");
        }

        // Check the inverse relationship
        if (objectF.ObjectE.ObjectF == null)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: objectF.ObjectE.ObjectF isn't null ");
        }

        if (objectE.Id != objectF.ObjectE.ObjectFKey || objectF.Bar != objectF.ObjectE.ObjectF.Bar)
        {
            return Tuple.Create(false, "TestGetInverseOneToOneRelationshipWithExplicitKey: Relationship data mismatch");
        }

        return Tuple.Create(true, "TestGetInverseOneToOneRelationshipWithExplicitKey: Passed");
    }

    public static Tuple<bool, string> TestUpdateSetOneToOneRelationship()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassA>();
        conn.DropTable<O2OClassB>();
        conn.CreateTable<O2OClassA>();
        conn.CreateTable<O2OClassB>();

        // Use standard SQLite-Net API to create a new relationship
        var objectB = new O2OClassB
        {
            Foo = string.Format("Foo String {0}", new Random().Next(100))
        };
        conn.Insert(objectB);

        var objectA = new O2OClassA();
        conn.Insert(objectA);

        // Set the relationship using objects
        objectA.OneClassB = objectB;
        if (objectA.OneClassBKey != 0)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationship failed: OneClassBKey should be 0 before update");
        }

        conn.UpdateWithChildren(objectA);

        if (objectA.OneClassBKey != objectB.Id)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationship failed: Foreign key should have been refreshed");
        }

        // Fetch the relationship
        var newObjectA = conn.Get<O2OClassA>(objectA.Id);

        if (newObjectA.OneClassBKey != objectB.Id)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationship failed: Foreign key in database is not updated");
        }

        return Tuple.Create(true, "TestUpdateSetOneToOneRelationship passed");
    }

    public static Tuple<bool, string> TestUpdateUnsetOneToOneRelationship()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassA>();
        conn.DropTable<O2OClassB>();
        conn.CreateTable<O2OClassA>();
        conn.CreateTable<O2OClassB>();

        // Use standard SQLite-Net API to create a new relationship
        var objectB = new O2OClassB
        {
            Foo = string.Format("Foo String {0}", new Random().Next(100))
        };
        conn.Insert(objectB);

        var objectA = new O2OClassA();
        conn.Insert(objectA);

        // Set the relationship using objects
        objectA.OneClassB = objectB;

        if (objectA.OneClassBKey != 0)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationship failed: Foreign key !=0");
        }

        conn.UpdateWithChildren(objectA);

        if (objectA.OneClassBKey != objectB.Id)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationship failed: Foreign key should have been refreshed");
        }

        // Until here, test is same that TestUpdateSetOneToOneRelationship
        objectA.OneClassB = null; // Unset relationship

        if (objectA.OneClassBKey != objectB.Id)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationship failed: Foreign key shouldn't have been refreshed yet");
        }

        conn.UpdateWithChildren(objectA);

        if (objectA.OneClassBKey != 0)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationship failed: Foreign key hasn't been unset");
        }

        return Tuple.Create(true, "TestUpdateUnsetOneToOneRelationship passed");
    }

    public static Tuple<bool, string> TestUpdateSetOneToOneRelationshipWithInverse()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassE>();
        conn.DropTable<O2OClassF>();
        conn.CreateTable<O2OClassE>();
        conn.CreateTable<O2OClassF>();

        var objectF = new O2OClassF
        {
            Bar = string.Format("Bar String {0}", new Random().Next(100))
        };
        conn.Insert(objectF);

        var objectE = new O2OClassE();
        conn.Insert(objectE);

        objectE.ObjectF = objectF;
        if (objectE.ObjectFKey != 0)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverse failed: ObjectFKey should be 0 before update");
        }

        conn.UpdateWithChildren(objectE);

        if (objectE.ObjectFKey != objectF.Id)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverse failed: Foreign key should have been refreshed");
        }

        if (!ReferenceEquals(objectF, objectE.ObjectF))
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverse failed: Inverse relationship hasn't been set");
        }

        var newObjectE = conn.Get<O2OClassE>(objectE.Id);

        if (newObjectE.ObjectFKey != objectF.Id)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverse failed: Foreign key in database is not updated");
        }

        return Tuple.Create(true, "TestUpdateSetOneToOneRelationshipWithInverse passed");
    }

    public static Tuple<bool, string> TestUpdateSetOneToOneRelationshipWithInverseForeignKey()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassF>();
        conn.DropTable<O2OClassE>();
        conn.CreateTable<O2OClassF>();
        conn.CreateTable<O2OClassE>();

        var objectF = new O2OClassF
        {
            Bar = string.Format("Bar String {0}", new Random().Next(100))
        };
        conn.Insert(objectF);

        var objectE = new O2OClassE();
        conn.Insert(objectE);

        objectF.ObjectE = objectE;
        if (objectE.ObjectFKey != 0)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKey failed: ObjectFKey should be 0 before update");
        }

        conn.UpdateWithChildren(objectF);

        if (objectE.ObjectFKey != objectF.Id)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKey failed: Foreign key should have been refreshed");
        }

        if (!ReferenceEquals(objectF, objectE.ObjectF))
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKey failed: Inverse relationship hasn't been set");
        }

        var newObjectE = conn.Get<O2OClassE>(objectE.Id);

        if (newObjectE.ObjectFKey != objectF.Id)
        {
            return Tuple.Create(false, "TestUpdateSetOneToOneRelationshipWithInverseForeignKey failed: Foreign key in database is not updated");
        }

        return Tuple.Create(true, "TestUpdateSetOneToOneRelationshipWithInverseForeignKey passed");
    }

    public static Tuple<bool, string> TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassF>();
        conn.DropTable<O2OClassE>();
        conn.CreateTable<O2OClassF>();
        conn.CreateTable<O2OClassE>();

        // Use standard SQLite-Net API to create a new relationship
        var objectF = new O2OClassF
        {
            Bar = string.Format("Bar String {0}", new Random().Next(100))
        };
        conn.Insert(objectF);

        var objectE = new O2OClassE();
        conn.Insert(objectE);

        // Set the relationship using objects
        objectF.ObjectE = objectE;
        if (objectE.ObjectFKey != 0)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey failed: Foreign key !=0");
        }

        conn.UpdateWithChildren(objectF);

        if (objectE.ObjectFKey != objectF.Id)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey failed: Foreign key should have been refreshed");
        }

        // At this point the test is the same as TestUpdateSetOneToOneRelationshipWithInverseForeignKey
        objectF.ObjectE = null;     // Unset the relationship

        conn.UpdateWithChildren(objectF);
        // Fetch the relationship
        var newObjectA = conn.Get<O2OClassE>(objectE.Id);
        if (newObjectA.ObjectFKey != 0)
        {
            return Tuple.Create(false, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey failed: Foreign key should have been refreshed in database");
        }

        return Tuple.Create(true, "TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey passed");
    }

    public static Tuple<bool, string> TestGetAllNoFilter()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassA>();
        conn.DropTable<O2OClassB>();
        conn.CreateTable<O2OClassA>();
        conn.CreateTable<O2OClassB>();

        var a1 = new O2OClassA();
        var a2 = new O2OClassA();
        var a3 = new O2OClassA();
        var aObjects = new[] { a1, a2, a3 };
        conn.InsertAll(aObjects);

        var b1 = new O2OClassB { Foo = "Foo 1" };
        var b2 = new O2OClassB { Foo = "Foo 2" };
        var b3 = new O2OClassB { Foo = "Foo 3" };
        var bObjects = new[] { b1, b2, b3 };
        conn.InsertAll(bObjects);

        a1.OneClassB = b1;
        a2.OneClassB = b2;
        a3.OneClassB = b3;
        conn.UpdateWithChildren(a1);
        conn.UpdateWithChildren(a2);
        conn.UpdateWithChildren(a3);

        var aElements = conn.GetAllWithChildren<O2OClassA>().OrderBy(a => a.Id).ToArray();

        if (aObjects.Length != aElements.Length)
        {
            return Tuple.Create(false, "TestGetAllNoFilter failed: Number of elements does not match");
        }

        for (int i = 0; i < aObjects.Length; i++)
        {
            if (aObjects[i].Id != aElements[i].Id)
            {
                return Tuple.Create(false, $"TestGetAllNoFilter failed: Id mismatch at index {i}");
            }

            if (aObjects[i].OneClassB!.Id != aElements[i].OneClassB!.Id)
            {
                return Tuple.Create(false, $"TestGetAllNoFilter failed: OneClassB Id mismatch at index {i}");
            }

            if (aObjects[i].OneClassB!.Foo != aElements[i].OneClassB!.Foo)
            {
                return Tuple.Create(false, $"TestGetAllNoFilter failed: OneClassB Foo mismatch at index {i}");
            }
        }

        return Tuple.Create(true, "TestGetAllNoFilter passed");
    }

    public static Tuple<bool, string> TestGetAllFilter()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<O2OClassC>();
        conn.DropTable<O2OClassD>();
        conn.CreateTable<O2OClassC>();
        conn.CreateTable<O2OClassD>();

        var c1 = new O2OClassC { Bar = "Bar 1" };
        var c2 = new O2OClassC { Bar = "Foo 2" };
        var c3 = new O2OClassC { Bar = "Bar 3" };
        var cObjects = new[] { c1, c2, c3 };
        conn.InsertAll(cObjects);

        var d1 = new O2OClassD { Foo = "Foo 1" };
        var d2 = new O2OClassD { Foo = "Foo 2" };
        var d3 = new O2OClassD { Foo = "Foo 3" };
        var bObjects = new[] { d1, d2, d3 };
        conn.InsertAll(bObjects);

        c1.ElementD = d1;
        c2.ElementD = d2;
        c3.ElementD = d3;
        conn.UpdateWithChildren(c1);
        conn.UpdateWithChildren(c2);
        conn.UpdateWithChildren(c3);

        var expectedCObjects = cObjects.Where(c => c.Bar.Contains("Bar")).ToArray();
        var cElements = conn.GetAllWithChildren<O2OClassC>(c => c.Bar.Contains("Bar"))
            .OrderBy(a => a.ClassId).ToArray();

        if (expectedCObjects.Length != cElements.Length)
        {
            return Tuple.Create(false, "TestGetAllFilter failed: Number of elements does not match");
        }

        for (int i = 0; i < expectedCObjects.Length; i++)
        {
            if (expectedCObjects[i].ClassId != cElements[i].ClassId)
            {
                return Tuple.Create(false, $"TestGetAllFilter failed: ClassId mismatch at index {i}");
            }

            if (expectedCObjects[i].ElementD!.Id != cElements[i].ElementD!.Id)
            {
                return Tuple.Create(false, $"TestGetAllFilter failed: ElementD Id mismatch at index {i}");
            }

            if (expectedCObjects[i].ElementD!.Foo != cElements[i].ElementD!.Foo)
            {
                return Tuple.Create(false, $"TestGetAllFilter failed: ElementD Foo mismatch at index {i}");
            }
        }

        return Tuple.Create(true, "TestGetAllFilter passed");
    }
}
