using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace IntegratedTestsSampleApp.Tests;

public class OneToManyTestsAsync
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

    public static async Task<Tuple<bool, string>> TestGetOneToManyListAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassA>();
            await conn.DropTableAsync<O2MClassB>();
            await conn.CreateTableAsync<O2MClassA>();
            await conn.CreateTableAsync<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await conn.InsertAsync(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListAsync: Initially BObjects should be null");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectA);
            if (objectA.BObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListAsync: BObjects should not be null after GetChildren");

            if (objectA.BObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListAsync: BObjects count should be 0 after GetChildren");

            // Set the relationship using IDs
            foreach (var objectB in objectsB)
            {
                objectB.ClassAKey = objectA.Id;
                await conn.UpdateAsync(objectB);
            }

            // Fetch the relationship
            await conn.GetChildrenAsync(objectA);

            if (objectA.BObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListAsync: BObjects should not be null after fetching relationship");

            if (objectA.BObjects.Count != objectsB.Count)
                return new Tuple<bool, string>(false, "TestGetOneToManyListAsync: BObjects count does not match expected count");

            var foos = objectsB.Select(objectB => objectB.Foo).ToList();
            foreach (var objectB in objectA.BObjects)
            {
                if (!foos.Contains(objectB.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyListAsync: Foo value for {objectB.Foo} not found in expected values");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyListAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyListAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetOneToManyListWithInverseAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassC>();
            await conn.DropTableAsync<O2MClassD>();
            await conn.CreateTableAsync<O2MClassC>();
            await conn.CreateTableAsync<O2MClassD>();

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassD>
        {
            new O2MClassD { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsD);

            var objectC = new O2MClassC();
            await conn.InsertAsync(objectC);

            if (objectC.DObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: Initially DObjects should be null");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectC);
            if (objectC.DObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: DObjects should not be null after GetChildren");

            if (objectC.DObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: DObjects count should be 0 after GetChildren");

            // Set the relationship using IDs
            foreach (var objectD in objectsD)
            {
                objectD.ClassCKey = objectC.Id;
                await conn.UpdateAsync(objectD);
            }

            // Fetch the relationship
            await conn.GetChildrenAsync(objectC);

            if (objectC.DObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: DObjects should not be null after fetching relationship");

            if (objectC.DObjects.Count != objectsD.Count)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: DObjects count does not match expected count");

            var foos = objectsD.Select(objectD => objectD.Foo).ToList();
            foreach (var objectD in objectC.DObjects)
            {
                if (!foos.Contains(objectD.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverseAsync: Foo value for {objectD.Foo} not found in expected values");

                if (objectC.Id != objectD.ObjectC.Id)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: ObjectC Id mismatch");

                if (objectC.Bar != objectD.ObjectC.Bar)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: ObjectC Bar mismatch");

                if (!ReferenceEquals(objectC, objectD.ObjectC))
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseAsync: ObjectC reference mismatch");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyListWithInverseAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverseAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetOneToManyArrayAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassE>();
            await conn.DropTableAsync<O2MClassF>();
            await conn.CreateTableAsync<O2MClassE>();
            await conn.CreateTableAsync<O2MClassF>();

            // Use standard SQLite-Net API to create the objects
            var objectsF = new[]
            {
            new O2MClassF { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
             };
            await conn.InsertAllAsync(objectsF);

            var objectE = new O2MClassE();
            await conn.InsertAsync(objectE);

            if (objectE.FObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyArrayAsync: Initially FObjects should be null");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectE);
            if (objectE.FObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyArrayAsync: FObjects should not be null after GetChildren");

            if (objectE.FObjects.Length != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyArrayAsync: FObjects count should be 0 after GetChildren");

            // Set the relationship using IDs
            foreach (var objectF in objectsF)
            {
                objectF.ClassEKey = objectE.Id;
                await conn.UpdateAsync(objectF);
            }

            // Fetch the relationship
            await conn.GetChildrenAsync(objectE);

            if (objectE.FObjects == null)
                return new Tuple<bool, string>(false, "TestGetOneToManyArrayAsync: FObjects should not be null after fetching relationship");

            if (objectE.FObjects.Length != objectsF.Length)
                return new Tuple<bool, string>(false, "TestGetOneToManyArrayAsync: FObjects count does not match expected count");

            var foos = objectsF.Select(objectF => objectF.Foo).ToList();
            foreach (var objectF in objectE.FObjects)
            {
                if (!foos.Contains(objectF.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyArrayAsync: Foo value for {objectF.Foo} not found in expected values");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyArrayAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyArrayAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToManyListAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassA>();
            await conn.DropTableAsync<O2MClassB>();
            await conn.CreateTableAsync<O2MClassA>();
            await conn.CreateTableAsync<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await conn.InsertAsync(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListAsync: Initially BObjects should be null");

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                if (objectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListAsync: Foreign keys shouldn't have been updated yet");
            }

            await conn.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                if (objectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListAsync: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = await conn.GetAsync<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListAsync: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyListAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyListAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateUnsetOneToManyEmptyListAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassA>();
            await conn.DropTableAsync<O2MClassB>();
            await conn.CreateTableAsync<O2MClassA>();
            await conn.CreateTableAsync<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
        {
            new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await conn.InsertAsync(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyListAsync: Initially BObjects should be null");

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                if (objectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyListAsync: Foreign keys shouldn't have been updated yet");
            }

            await conn.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                if (objectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyListAsync: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = await conn.GetAsync<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyListAsync: Database stored value is not correct");
            }

            // Reset the relationship
            objectA.BObjects = new List<O2MClassB>();

            await conn.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectsB)
            {
                var newObjectB = await conn.GetAsync<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyEmptyListAsync: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateUnsetOneToManyEmptyListAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetOneToManyEmptyListAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateUnsetOneToManyNullListAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassA>();
            await conn.DropTableAsync<O2MClassB>();
            await conn.CreateTableAsync<O2MClassA>();
            await conn.CreateTableAsync<O2MClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
            {
                new O2MClassB { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
                new O2MClassB { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
                new O2MClassB { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
                new O2MClassB { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
            };
            await conn.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await conn.InsertAsync(objectA);

            if (objectA.BObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Initially BObjects should be null");

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                if (objectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Foreign keys shouldn't have been updated yet");
            }

            await conn.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                if (objectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = await conn.GetAsync<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != objectA.Id)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Database stored value is not correct");
            }

            // Reset the relationship to null
            objectA.BObjects = null;

            await conn.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectsB)
            {
                var newObjectB = await conn.GetAsync<O2MClassB>(objectB.Id);
                if (newObjectB.ClassAKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateUnsetOneToManyNullList: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateUnsetOneToManyNullList: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetOneToManyNullListAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToManyArrayAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassE>();
            await conn.DropTableAsync<O2MClassF>();
            await conn.CreateTableAsync<O2MClassE>();
            await conn.CreateTableAsync<O2MClassF>();

            // Use standard SQLite-Net API to create the objects
            var objectsF = new[]
            {
            new O2MClassF { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassF { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
            };
            await conn.InsertAllAsync(objectsF);

            var objectE = new O2MClassE();
            await conn.InsertAsync(objectE);

            if (objectE.FObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Initially FObjects should be null");

            objectE.FObjects = objectsF;

            foreach (var objectF in objectsF)
            {
                if (objectF.ClassEKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Foreign keys shouldn't have been updated yet");
            }

            await conn.UpdateWithChildrenAsync(objectE);

            foreach (var objectF in objectE.FObjects)
            {
                if (objectF.ClassEKey != objectE.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Foreign keys haven't been updated yet");

                // Check database values
                var newObjectF = await conn.GetAsync<O2MClassF>(objectF.Id);
                if (newObjectF.ClassEKey != objectE.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyArray: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyArray: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyArrayAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToManyListWithInverseAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<O2MClassC>();
            await conn.DropTableAsync<O2MClassD>();
            await conn.CreateTableAsync<O2MClassC>();
            await conn.CreateTableAsync<O2MClassD>();

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassD>
        {
            new O2MClassD { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassD { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsD);

            var objectC = new O2MClassC();
            await conn.InsertAsync(objectC);

            if (objectC.DObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Initially DObjects should be null");

            objectC.DObjects = new ObservableCollection<O2MClassD>(objectsD);

            foreach (var objectD in objectsD)
            {
                if (objectD.ClassCKey != 0)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Foreign keys shouldn't have been updated yet");
            }

            await conn.UpdateWithChildrenAsync(objectC);

            foreach (var objectD in objectC.DObjects)
            {
                if (objectD.ClassCKey != objectC.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Foreign keys haven't been updated yet");
                if (objectD.ObjectC != objectC)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Inverse relationship hasn't been set");

                // Check database values
                var newObjectD = await conn.GetAsync<O2MClassD>(objectD.Id);
                if (newObjectD.ClassCKey != objectC.Id)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverse: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyListWithInverse: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyListWithInverseAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetOneToManyListWithInverseGuidIdAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();

            await conn.DropTableAsync<O2MClassG>(); // Async drop table
            await conn.DropTableAsync<O2MClassH>(); // Async drop table
            await conn.CreateTableAsync<O2MClassG>(); // Async create table
            await conn.CreateTableAsync<O2MClassH>(); // Async create table

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassH>
        {
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsD); // Async insert

            var objectC = new O2MClassG { Guid = Guid.NewGuid() };
            await conn.InsertAsync(objectC); // Async insert

            if (objectC.HObjects != null)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: Initially HObjects should be null");

            // Fetch (yet empty) the relationship
            await conn.GetChildrenAsync(objectC); // Async fetch
            if (objectC.HObjects == null || objectC.HObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: HObjects should be empty after GetChildren");

            // Set the relationship using IDs
            foreach (var objectD in objectsD)
            {
                objectD.ClassGKey = objectC.Guid;
                await conn.UpdateAsync(objectD); // Async update
            }

            if (objectC.HObjects == null || objectC.HObjects.Count != 0)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: HObjects should still be empty");

            // Fetch the relationship
            await conn.GetChildrenAsync(objectC); // Async fetch

            if (objectC.HObjects == null || objectC.HObjects.Count != objectsD.Count)
                return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: HObjects count does not match the expected count");

            var foos = objectsD.Select(objectD => objectD.Foo).ToList();
            foreach (var objectD in objectC.HObjects)
            {
                if (!foos.Contains(objectD.Foo))
                    return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverseGuidIdAsync: Foo value for {objectD.Foo} not found in expected values");
                if (objectD.ObjectG.Guid != objectC.Guid)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: ObjectG.Guid doesn't match");
                if (objectD.ObjectG.Bar != objectC.Bar)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: ObjectG.Bar doesn't match");
                if (objectD.ObjectG != objectC)
                    return new Tuple<bool, string>(false, "TestGetOneToManyListWithInverseGuidIdAsync: ObjectG is not the same as objectC");
            }

            return new Tuple<bool, string>(true, "TestGetOneToManyListWithInverseGuidIdAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetOneToManyListWithInverseGuidIdAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetOneToManyListWithInverseGuidIdAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();

            await conn.DropTableAsync<O2MClassG>(); // Async drop table
            await conn.DropTableAsync<O2MClassH>(); // Async drop table
            await conn.CreateTableAsync<O2MClassG>(); // Async create table
            await conn.CreateTableAsync<O2MClassH>(); // Async create table

            // Use standard SQLite-Net API to create the objects
            var objectsH = new List<O2MClassH>
            {
                new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
                new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
                new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
                new O2MClassH { Guid = Guid.NewGuid(), Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
            };
            await conn.InsertAllAsync(objectsH); // Async insert

            var objectG = new O2MClassG { Guid = Guid.NewGuid() };
            await conn.InsertAsync(objectG); // Async insert

            if (objectG.HObjects != null)
                return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidIdAsync: Initially HObjects should be null");

            objectG.HObjects = new ObservableCollection<O2MClassH>(objectsH);

            foreach (var objectH in objectsH)
            {
                if (objectH.ClassGKey != Guid.Empty)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidIdAsync: Foreign keys shouldn't have been updated yet");
            }

            await conn.UpdateWithChildrenAsync(objectG); // Async update

            foreach (var objectH in objectG.HObjects)
            {
                if (objectH.ClassGKey != objectG.Guid)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidIdAsync: Foreign keys haven't been updated yet");
                if (objectH.ObjectG != objectG)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidIdAsync: Inverse relationship hasn't been set");

                // Check database values
                var newObjectH = await conn.GetAsync<O2MClassH>(objectH.Guid); // Async get
                if (newObjectH.ClassGKey != objectG.Guid)
                    return new Tuple<bool, string>(false, "TestUpdateSetOneToManyListWithInverseGuidIdAsync: Database stored value is not correct");
            }

            return new Tuple<bool, string>(true, "TestUpdateSetOneToManyListWithInverseGuidIdAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetOneToManyListWithInverseGuidIdAsync: Test failed with exception: {ex.Message}");
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

    public static async Task<Tuple<bool, string>> TestRecursiveInverseRelationshipAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Employee>();
            await conn.CreateTableAsync<Employee>();

            var employee1 = new Employee { Name = "Albert" };
            await conn.InsertAsync(employee1);

            var employee2 = new Employee
            {
                Name = "Leonardo",
                SupervisorId = employee1.Id
            };
            await conn.InsertAsync(employee2);

            var result = await conn.GetWithChildrenAsync<Employee>(employee1.Id);

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
