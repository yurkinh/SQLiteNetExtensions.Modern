using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace IntegratedTestsSampleApp.Tests;

public class OneToManyTests
{
    [Table("ClassA")]
    public class O2MClassA
    {
        [PrimaryKey, AutoIncrement, Column("PrimaryKey")]
        public int Id { get; set; }

        [OneToMany]
        public List<O2MClassB> BObjects { get; set; }

        public string Bar { get; set; }
    }

    [Table("ClassB")]
    public class O2MClassB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(O2MClassA)), Column("class_a_id")]
        public int ClassAKey { get; set; }

        public string Foo { get; set; }
    }

    public class O2MClassC
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [OneToMany]
        public ObservableCollection<O2MClassD> DObjects { get; set; }

        public string Bar { get; set; }
    }

    public class O2MClassD
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(O2MClassC))]
        public int ClassCKey { get; set; }

        [ManyToOne]     // OneToMany Inverse relationship
        public O2MClassC ObjectC { get; set; }

        public string Foo { get; set; }
    }

    public class O2MClassE
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [OneToMany("ClassEKey")]   // Explicit foreign key declaration
        public O2MClassF[] FObjects { get; set; } // Array of objects instead of List

        public string Bar { get; set; }
    }

    public class O2MClassF
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ClassEKey { get; set; }  // Foreign key declared in relationship

        public string Foo { get; set; }
    }

    public class O2MClassG
    {
        [PrimaryKey]
        public Guid Guid { get; set; }

        [OneToMany]
        public ObservableCollection<O2MClassH> HObjects { get; set; }

        public string Bar { get; set; }
    }

    public class O2MClassH
    {
        [PrimaryKey]
        public Guid Guid { get; set; }

        [ForeignKey(typeof(O2MClassG))]
        public Guid ClassGKey { get; set; }

        [ManyToOne]     // OneToMany Inverse relationship
        public O2MClassG ObjectG { get; set; }

        public string Foo { get; set; }
    }

    public static Tuple<bool, string> TestGetOneToManyList()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassA>();
            conn.DropTable<O2MClassB>();
            conn.CreateTable<O2MClassA>();
            conn.CreateTable<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsB);

            var objectA = new O2MClassA();
            conn.Insert(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyList: Initially BObjects should be null");

            // Fetch (yet empty) the relationship
            conn.GetChildren(objectA);
            if (objectA.BObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyList: BObjects should not be null after GetChildren");

            if (objectA.BObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyList: BObjects count should be 0 after GetChildren");

            // Set the relationship using IDs
            foreach (var objectB in objectsB)
            {
                objectB.ClassAKey = objectA.Id;
                conn.Update(objectB);
            }

            // Fetch the relationship
            conn.GetChildren(objectA);

            if (objectA.BObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyList: BObjects should not be null after fetching relationship");

            if (objectA.BObjects.Count != objectsB.Count)
                return new Tuple<bool, string>(false, "TestGetOneToManyList: BObjects count does not match expected count");

            var foos = objectsB.Select(objectB => objectB.Foo).ToList();
            foreach (var objectB in objectA.BObjects)
            {
                if (!foos.Contains(objectB.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyList: Foo value for {objectB.Foo} not found in expected values");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyList: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyList: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestGetOneToManyListWithInverse()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassC>();
            conn.DropTable<O2MClassD>();
            conn.CreateTable<O2MClassC>();
            conn.CreateTable<O2MClassD>();

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassD>
        {
            new O2MClassD { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsD);

            var objectC = new O2MClassC();
            conn.Insert(objectC);

            if (objectC.DObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: Initially DObjects should be null");

            // Fetch (yet empty) the relationship
            conn.GetChildren(objectC);
            if (objectC.DObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: DObjects should not be null after GetChildren");

            if (objectC.DObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: DObjects count should be 0 after GetChildren");

            // Set the relationship using IDs
            foreach (var objectD in objectsD)
            {
                objectD.ClassCKey = objectC.Id;
                conn.Update(objectD);
            }

            // Fetch the relationship
            conn.GetChildren(objectC);

            if (objectC.DObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: DObjects should not be null after fetching relationship");

            if (objectC.DObjects.Count != objectsD.Count)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: DObjects count does not match expected count");

            var foos = objectsD.Select(objectD => objectD.Foo).ToList();
            foreach (var objectD in objectC.DObjects)
            {
                if (!foos.Contains(objectD.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverse: Foo value for {objectD.Foo} not found in expected values");

                if (objectC.Id != objectD.ObjectC.Id)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: ObjectC Id mismatch");

                if (objectC.Bar != objectD.ObjectC.Bar)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: ObjectC Bar mismatch");

                if (!ReferenceEquals(objectC, objectD.ObjectC))
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverse: ObjectC reference mismatch");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyListWithInverse: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverse: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestGetOneToManyArray()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassE>();
            conn.DropTable<O2MClassF>();
            conn.CreateTable<O2MClassE>();
            conn.CreateTable<O2MClassF>();

            // Use standard SQLite-Net API to create the objects
            var objectsF = new[]
            {
            new O2MClassF { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsF);

            var objectE = new O2MClassE();
            conn.Insert(objectE);

            if (objectE.FObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyArray: Initially FObjects should be null");

            // Fetch (yet empty) the relationship
            conn.GetChildren(objectE);
            if (objectE.FObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyArray: FObjects should not be null after GetChildren");

            if (objectE.FObjects.Length != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyArray: FObjects count should be 0 after GetChildren");

            // Set the relationship using IDs
            foreach (var objectF in objectsF)
            {
                objectF.ClassEKey = objectE.Id;
                conn.Update(objectF);
            }

            // Fetch the relationship
            conn.GetChildren(objectE);

            if (objectE.FObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyArray: FObjects should not be null after fetching relationship");

            if (objectE.FObjects.Length != objectsF.Length)
                return new Tuple<bool, string>(false, "TestGetOneToManyArray: FObjects count does not match expected count");

            var foos = objectsF.Select(objectF => objectF.Foo).ToList();
            foreach (var objectF in objectE.FObjects)
            {
                if (!foos.Contains(objectF.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyArray: Foo value for {objectF.Foo} not found in expected values");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyArray: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyArray: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateSetOneToManyList()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassA>();
            conn.DropTable<O2MClassB>();
            conn.CreateTable<O2MClassA>();
            conn.CreateTable<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsB);

            var objectA = new O2MClassA();
            conn.Insert(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyList: Initially BObjects should be null");

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                if (objectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyList: Foreign keys shouldn't have been updated yet");
            }

            conn.UpdateWithChildren(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                if (objectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyList: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = conn.Get<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyList: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyList: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyList: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateUnsetOneToManyEmptyList()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassA>();
            conn.DropTable<O2MClassB>();
            conn.CreateTable<O2MClassA>();
            conn.CreateTable<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsB);

            var objectA = new O2MClassA();
            conn.Insert(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyList: Initially BObjects should be null");

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                if (objectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyList: Foreign keys shouldn't have been updated yet");
            }

            conn.UpdateWithChildren(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                if (objectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyList: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = conn.Get<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyList: Database stored value is not correct");
            }

            // Reset the relationship
            objectA.BObjects = new List<O2MClassB>();

            conn.UpdateWithChildren(objectA);

            foreach (var objectB in objectsB)
            {
                var newObjectB = conn.Get<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyList: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateUnsetOneToManyEmptyList: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetOneToManyEmptyList: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateUnsetOneToManyNullList()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassA>();
            conn.DropTable<O2MClassB>();
            conn.CreateTable<O2MClassA>();
            conn.CreateTable<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsB);

            var objectA = new O2MClassA();
            conn.Insert(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Initially BObjects should be null");

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                if (objectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Foreign keys shouldn't have been updated yet");
            }

            conn.UpdateWithChildren(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                if (objectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = conn.Get<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Database stored value is not correct");
            }

            // Reset the relationship to null
            objectA.BObjects = null;

            conn.UpdateWithChildren(objectA);

            foreach (var objectB in objectsB)
            {
                var newObjectB = conn.Get<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateUnsetOneToManyNullList: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetOneToManyNullList: Test failed with exception: {ex.Message}");
        }
    }
    
    public static Tuple<bool, string> TestUpdateSetOneToManyArray()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassE>();
            conn.DropTable<O2MClassF>();
            conn.CreateTable<O2MClassE>();
            conn.CreateTable<O2MClassF>();

            // Use standard SQLite-Net API to create the objects
            var objectsF = new[]
            {
            new O2MClassF { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsF);

            var objectE = new O2MClassE();
            conn.Insert(objectE);

            if (objectE.FObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Initially FObjects should be null");

            objectE.FObjects = objectsF;

            foreach (var objectF in objectsF)
            {
                if (objectF.ClassEKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Foreign keys shouldn't have been updated yet");
            }

            conn.UpdateWithChildren(objectE);

            foreach (var objectF in objectE.FObjects)
            {
                if (objectF.ClassEKey != objectE.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectF = conn.Get<O2MClassF>(objectF.Id);
                if (newObjectF.ClassEKey != objectE.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyArray: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyArray: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateSetOneToManyListWithInverse()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassC>();
            conn.DropTable<O2MClassD>();
            conn.CreateTable<O2MClassC>();
            conn.CreateTable<O2MClassD>();

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassD>
        {
            new O2MClassD { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsD);

            var objectC = new O2MClassC();
            conn.Insert(objectC);

            if (objectC.DObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Initially DObjects should be null");

            objectC.DObjects = new ObservableCollection<O2MClassD>(objectsD);

            foreach (var objectD in objectsD)
            {
                if (objectD.ClassCKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Foreign keys shouldn't have been updated yet");
            }

            conn.UpdateWithChildren(objectC);

            foreach (var objectD in objectC.DObjects)
            {
                if (objectD.ClassCKey != objectC.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Foreign keys haven't been updated yet");
                if (objectD.ObjectC != objectC)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Inverse relationship hasn't been set");

                // Check database values
                var newObjectD = conn.Get<O2MClassD>(objectD.Id);
                if (newObjectD.ClassCKey != objectC.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyListWithInverse: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyListWithInverse: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestGetOneToManyListWithInverseGuidId()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassG>();
            conn.DropTable<O2MClassH>();
            conn.CreateTable<O2MClassG>();
            conn.CreateTable<O2MClassH>();

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassH>
        {
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsD);

            var objectC = new O2MClassG { Guid = Guid.NewGuid() };
            conn.Insert(objectC);

            if (objectC.HObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: Initially HObjects should be null");

            // Fetch (yet empty) the relationship
            conn.GetChildren(objectC);
            if (objectC.HObjects == null || objectC.HObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: HObjects should be empty after GetChildren");

            // Set the relationship using IDs
            foreach (var objectD in objectsD)
            {
                objectD.ClassGKey = objectC.Guid;
                conn.Update(objectD);
            }

            if (objectC.HObjects == null || objectC.HObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: HObjects should still be empty");

            // Fetch the relationship
            conn.GetChildren(objectC);

            if (objectC.HObjects == null || objectC.HObjects.Count != objectsD.Count)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: HObjects count does not match the expected count");

            var foos = objectsD.Select(objectD => objectD.Foo).ToList();
            foreach (var objectD in objectC.HObjects)
            {
                if (!foos.Contains(objectD.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverseGuidId: Foo value for {objectD.Foo} not found in expected values");
                if (objectD.ObjectG.Guid != objectC.Guid)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: ObjectG.Guid doesn't match");
                if (objectD.ObjectG.Bar != objectC.Bar)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: ObjectG.Bar doesn't match");
                if (objectD.ObjectG != objectC)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidId: ObjectG is not the same as objectC");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyListWithInverseGuidId: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverseGuidId: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateSetOneToManyListWithInverseGuidId()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<O2MClassG>();
            conn.DropTable<O2MClassH>();
            conn.CreateTable<O2MClassG>();
            conn.CreateTable<O2MClassH>();

            // Use standard SQLite-Net API to create the objects
            var objectsH = new List<O2MClassH>
        {
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            conn.InsertAll(objectsH);

            var objectG = new O2MClassG { Guid = Guid.NewGuid() };
            conn.Insert(objectG);

            if (objectG.HObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidId: Initially HObjects should be null");

            objectG.HObjects = new ObservableCollection<O2MClassH>(objectsH);

            foreach (var objectH in objectsH)
            {
                if (objectH.ClassGKey != Guid.Empty)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidId: Foreign keys shouldn't have been updated yet");
            }

            conn.UpdateWithChildren(objectG);

            foreach (var objectH in objectG.HObjects)
            {
                if (objectH.ClassGKey != objectG.Guid)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidId: Foreign keys haven't been updated yet");
                if (objectH.ObjectG != objectG)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidId: Inverse relationship hasn't been set");

                // Check database values
                var newObjectH = conn.Get<O2MClassH>(objectH.Guid);
                if (newObjectH.ClassGKey != objectG.Guid)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidId: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyListWithInverseGuidId: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyListWithInverseGuidId: Test failed with exception: {ex.Message}");
        }
    }

    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        [OneToMany]
        public List<Employee> Subordinates { get; set; }

        [ManyToOne]
        public Employee Supervisor { get; set; }

        [ForeignKey(typeof(Employee))]
        public int SupervisorId { get; set; }
    }

    public static Tuple<bool, string> TestRecursiveInverseRelationship()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<Employee>();
            conn.CreateTable<Employee>();

            var employee1 = new Employee { Name = "Albert" };
            conn.Insert(employee1);

            var employee2 = new Employee
            {
                Name = "Leonardo",
                SupervisorId = employee1.Id
            };
            conn.Insert(employee2);

            var result = conn.GetWithChildren<Employee>(employee1.Id);

            if (!employee1.Equals(result))
                return new Tuple<bool, string>(false, "TestRecursiveInverseRelationship: Retrieved employee is not the same as the expected employee.");

            if (!employee1.Subordinates.Select(e => e.Name).Contains(employee2.Name))
                return new Tuple<bool, string>(false, "TestRecursiveInverseRelationship: Subordinates list does not contain the expected subordinate.");

            return new Tuple<bool, string>(true, "TestRecursiveInverseRelationship: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestRecursiveInverseRelationship: Test failed with exception: {ex.Message}");
        }
    }
}
