using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace IntegratedTestsSampleApp.Tests;

public class ManyToManyTestsAsync
{
    public class M2MClassA
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [ManyToMany(typeof(ClassAClassB))]
        public List<M2MClassB> BObjects { get; set; }

        public string Bar { get; set; }
    }

    public class M2MClassB
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    public class ClassAClassB
    {
        [ForeignKey(typeof(M2MClassA)), Column("class_a_id")]
        public int ClassAId { get; set; }

        [ForeignKey(typeof(M2MClassB))]
        public int ClassBId { get; set; }
    }

    public class M2MClassC
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ManyToMany(typeof(ClassCClassD), inverseForeignKey: "ClassCId")]   // Foreign key specified in ManyToMany attribute
        public M2MClassD[] DObjects { get; set; } // Array instead of List

        public string Bar { get; set; }
    }

    public class M2MClassD
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    public class ClassCClassD
    {
        public int ClassCId { get; set; }   // ForeignKey attribute not needed, already specified in the ManyToMany relationship
        [ForeignKey(typeof(M2MClassD))]
        public int ClassDId { get; set; }
    }

    [Table("class_e")]
    public class M2MClassE
    {
        [PrimaryKey]
        public Guid Id { get; set; } // Guid identifier instead of int

        [ManyToMany(typeof(ClassEClassF), inverseForeignKey: "ClassEId")]   // Foreign key specified in ManyToMany attribute
        public M2MClassF[] FObjects { get; set; } // Array instead of List

        public string Bar { get; set; }
    }

    public class M2MClassF
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    [Table("class_e_class_f")]
    public class ClassEClassF
    {
        public Guid ClassEId { get; set; }   // ForeignKey attribute not needed, already specified in the ManyToMany relationship
        [ForeignKey(typeof(M2MClassF))]
        public int ClassFId { get; set; }
    }

    public class M2MClassG
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        [ManyToMany(typeof(ClassGClassG), "ChildId", "Children")]
        public ObservableCollection<M2MClassG> Parents { get; set; }

        [ManyToMany(typeof(ClassGClassG), "ParentId", "Parents")]
        public List<M2MClassG> Children { get; set; }
    }

    [Table("M2MClassG_ClassG")]
    public class ClassGClassG
    {
        [Column("Identifier")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("parent_id")]
        public int ParentId { get; set; }
        public int ChildId { get; set; }
    }

    [Table("ClassH")]
    public class M2MClassH
    {
        [Column("_id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        [Column("parent_elements")]
        [ManyToMany(typeof(ClassHClassH), "ChildId", "Children", ReadOnly = true)] // Parents relationship is read only
        public List<M2MClassH> Parents { get; set; }

        [ManyToMany(typeof(ClassHClassH), "ParentId", "Parents")]
        public ObservableCollection<M2MClassH> Children { get; set; }
    }

    public class ClassHClassH
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ParentId { get; set; }
        public int ChildId { get; set; }
    }

    public static async Task<Tuple<bool, string>> TestGetManyToManyListAsync()
    {
        try
        {
            // In this test we will create a N:M relationship between objects of ClassA and ClassB
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassA>();
            await conn.DropTableAsync<M2MClassB>();
            await conn.DropTableAsync<ClassAClassB>();
            await conn.CreateTableAsync<M2MClassA>();
            await conn.CreateTableAsync<M2MClassB>();
            await conn.CreateTableAsync<ClassAClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<M2MClassB>
        {
            new() { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new() { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new() { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new() { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsB);

            var objectsA = new List<M2MClassA>
        {
            new() { Bar = string.Format("1- Bar String {0}", new Random().Next(100)) },
            new() { Bar = string.Format("2- Bar String {0}", new Random().Next(100)) },
            new() { Bar = string.Format("3- Bar String {0}", new Random().Next(100)) },
            new() { Bar = string.Format("4- Bar String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsA);

            foreach (var objectA in objectsA)
            {
                var copyA = objectA;
                if (objectA.BObjects != null)
                    return new Tuple<bool, string>(false, "TestGetManyToManyList: Failed at checking initial BObjects null");

                // Fetch (yet empty) the relationship
                await conn.GetChildrenAsync(copyA);

                if (copyA.BObjects == null || copyA.BObjects.Count != 0)
                    return new Tuple<bool, string>(false, "TestGetManyToManyList: Failed at fetching empty relationship");
            }

            // Create the relationships in the intermediate table
            for (var aIndex = 0; aIndex < objectsA.Count; aIndex++)
            {
                for (var bIndex = 0; bIndex <= aIndex; bIndex++)
                {
                    await conn.InsertAsync(new ClassAClassB
                    {
                        ClassAId = objectsA[aIndex].Id,
                        ClassBId = objectsB[bIndex].Id
                    });
                }
            }

            for (var i = 0; i < objectsA.Count; i++)
            {
                var objectA = objectsA[i];

                // Relationship still empty because hasn't been refreshed
                if (objectA.BObjects == null || objectA.BObjects.Count != 0)
                    return new Tuple<bool, string>(false, "TestGetManyToManyList: Failed at checking relationship before refresh");

                // Fetch the relationship
                await conn.GetChildrenAsync(objectA);

                var childrenCount = i + 1;

                if (objectA.BObjects == null || objectA.BObjects.Count != childrenCount)
                    return new Tuple<bool, string>(false, "TestGetManyToManyList: Failed at fetching relationship after update");

                var foos = objectsB.GetRange(0, childrenCount).Select(objectB => objectB.Foo).ToList();
                foreach (var objectB in objectA.BObjects)
                {
                    if (!foos.Contains(objectB.Foo))
                        return new Tuple<bool, string>(false, "TestGetManyToManyList: Relationship data mismatch");
                }
            }

            return new Tuple<bool, string>(true, "TestGetManyToManyList: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetManyToManyList: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetManyToManyArrayAsync()
    {
        try
        {
            // In this test we will create a N:M relationship between objects of ClassC and ClassD
            //      Class C     -       Class D
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassC>();
            await conn.DropTableAsync<M2MClassD>();
            await conn.DropTableAsync<ClassCClassD>();
            await conn.CreateTableAsync<M2MClassC>();
            await conn.CreateTableAsync<M2MClassD>();
            await conn.CreateTableAsync<ClassCClassD>();

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<M2MClassD>
        {
            new() { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new() { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new() { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new() { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsD);

            var objectsC = new List<M2MClassC>
        {
            new() { Bar = string.Format("1- Bar String {0}", new Random().Next(100)) },
            new() { Bar = string.Format("2- Bar String {0}", new Random().Next(100)) },
            new() { Bar = string.Format("3- Bar String {0}", new Random().Next(100)) },
            new() { Bar = string.Format("4- Bar String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsC);

            foreach (var objectC in objectsC)
            {
                var copyC = objectC;
                if (objectC.DObjects != null)
                    return new Tuple<bool, string>(false, "TestGetManyToManyArray: Failed at checking initial DObjects null");

                // Fetch (yet empty) the relationship
                await conn.GetChildrenAsync(copyC);

                if (copyC.DObjects == null || copyC.DObjects.Length != 0)
                    return new Tuple<bool, string>(false, "TestGetManyToManyArray: Failed at fetching empty relationship");
            }

            // Create the relationships in the intermediate table
            for (var cIndex = 0; cIndex < objectsC.Count; cIndex++)
            {
                for (var dIndex = 0; dIndex <= cIndex; dIndex++)
                {
                    await conn.InsertAsync(new ClassCClassD
                    {
                        ClassCId = objectsC[cIndex].Id,
                        ClassDId = objectsD[dIndex].Id
                    });
                }
            }

            for (var i = 0; i < objectsC.Count; i++)
            {
                var objectC = objectsC[i];

                // Relationship still empty because hasn't been refreshed
                if (objectC.DObjects == null || objectC.DObjects.Length != 0)
                    return new Tuple<bool, string>(false, "TestGetManyToManyArray: Failed at checking relationship before refresh");

                // Fetch the relationship
                await conn.GetChildrenAsync(objectC);

                var childrenCount = i + 1;

                if (objectC.DObjects == null || objectC.DObjects.Length != childrenCount)
                    return new Tuple<bool, string>(false, "TestGetManyToManyArray: Failed at fetching relationship after update");

                var foos = objectsD.GetRange(0, childrenCount).Select(objectD => objectD.Foo).ToList();
                foreach (var objectD in objectC.DObjects)
                {
                    if (!foos.Contains(objectD.Foo))
                        return new Tuple<bool, string>(false, "TestGetManyToManyArray: Relationship data mismatch");
                }
            }

            return new Tuple<bool, string>(true, "TestGetManyToManyArray: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetManyToManyArray: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetManyToManyListAsync()
    {
        try
        {
            // In this test we will create a N:M relationship between objects of ClassA and ClassB
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassA>();
            await conn.DropTableAsync<M2MClassB>();
            await conn.DropTableAsync<ClassAClassB>();
            await conn.CreateTableAsync<M2MClassA>();
            await conn.CreateTableAsync<M2MClassB>();
            await conn.CreateTableAsync<ClassAClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<M2MClassB>
        {
            new () { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
            new () { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
            new () { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
            new () { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
        };
            await conn.InsertAllAsync(objectsB);

            var objectsA = new List<M2MClassA>
        {
            new () { Bar = string.Format("1- Bar String {0}", new Random().Next(100)), BObjects = [] },
            new () { Bar = string.Format("2- Bar String {0}", new Random().Next(100)), BObjects = [] },
            new () { Bar = string.Format("3- Bar String {0}", new Random().Next(100)), BObjects = [] },
            new () { Bar = string.Format("4- Bar String {0}", new Random().Next(100)), BObjects = [] }
        };
            await conn.InsertAllAsync(objectsA);

            // Create the relationships
            for (var aIndex = 0; aIndex < objectsA.Count; aIndex++)
            {
                var objectA = objectsA[aIndex];

                for (var bIndex = 0; bIndex <= aIndex; bIndex++)
                {
                    var objectB = objectsB[bIndex];
                    objectA.BObjects.Add(objectB);
                }

                await conn.UpdateWithChildrenAsync(objectA);
            }

            // Verify the relationships
            for (var i = 0; i < objectsA.Count; i++)
            {
                var objectA = objectsA[i];
                var childrenCount = i + 1;

                var allRelationships = await conn.Table<ClassAClassB>().ToListAsync();
                var storedChildKeyList = allRelationships
                    .Where(ab => ab.ClassAId == objectA.Id)
                    .Select(ab => ab.ClassBId)
                    .ToList();

                if (childrenCount != storedChildKeyList.Count())
                    return new Tuple<bool, string>(false, "TestUpdateSetManyToManyListAsync: Relationship count is not correct");

                var expectedChildIds = objectsB.GetRange(0, childrenCount).Select(objectB => objectB.Id).ToList();
                foreach (var objectBKey in storedChildKeyList)
                {
                    if (!expectedChildIds.Contains(objectBKey))
                        return new Tuple<bool, string>(false, "TestUpdateSetManyToManyListAsync: Relationship ID is not correct");
                }
            }

            return new Tuple<bool, string>(true, "TestUpdateSetManyToManyListAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetManyToManyListAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateUnsetManyToManyListAsync()
    {
        try
        {
            // In this test we will create a N:M relationship between objects of ClassA and ClassB
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // After that, we will remove objects 1 and 2 from relationships
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       <empty>
            //          2       -       <empty>
            //          3       -       3
            //          4       -       3, 4

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassA>();
            await conn.DropTableAsync<M2MClassB>();
            await conn.DropTableAsync<ClassAClassB>();
            await conn.CreateTableAsync<M2MClassA>();
            await conn.CreateTableAsync<M2MClassB>();
            await conn.CreateTableAsync<ClassAClassB>();

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<M2MClassB>
            {
                new () { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
                new () { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
                new () { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
                new () { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
            };
            await conn.InsertAllAsync(objectsB);

            var objectsA = new List<M2MClassA>
            {
                new () { Bar = string.Format("1- Bar String {0}", new Random().Next(100)), BObjects = [] },
                new () { Bar = string.Format("2- Bar String {0}", new Random().Next(100)), BObjects = [] },
                new () { Bar = string.Format("3- Bar String {0}", new Random().Next(100)), BObjects = [] },
                new () { Bar = string.Format("4- Bar String {0}", new Random().Next(100)), BObjects = [] }
            };
            await conn.InsertAllAsync(objectsA);

            // Create the relationships
            for (var aIndex = 0; aIndex < objectsA.Count; aIndex++)
            {
                var objectA = objectsA[aIndex];

                for (var bIndex = 0; bIndex <= aIndex; bIndex++)
                {
                    var objectB = objectsB[bIndex];
                    objectA.BObjects.Add(objectB);
                }

                await conn.UpdateWithChildrenAsync(objectA);
            }

            // At these points all the relationships are set
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // Now we will remove ClassB objects 1 and 2 from the relationships
            var objectsBToRemove = objectsB.GetRange(0, 2);

            foreach (var objectA in objectsA)
            {
                objectA.BObjects.RemoveAll(objectsBToRemove.Contains);
                await conn.UpdateWithChildrenAsync(objectA);
            }

            // This should now be the current status of all relationships
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       <empty>
            //          2       -       <empty>
            //          3       -       3
            //          4       -       3, 4

            // Verify the updated relationships
            for (var i = 0; i < objectsA.Count; i++)
            {
                var objectA = objectsA[i];
                var allRelationships = await conn.Table<ClassAClassB>().ToListAsync();
                var storedChildKeyList = allRelationships
                    .Where(ab => ab.ClassAId == objectA.Id)
                    .Select(ab => ab.ClassBId)
                    .ToList();

                var expectedChildIds = objectsB.GetRange(0, i + 1)
                    .Where(b => !objectsBToRemove.Contains(b))
                    .Select(objectB => objectB.Id)
                    .ToList();

                if (expectedChildIds.Count != storedChildKeyList.Count)
                    return new Tuple<bool, string>(false, $"TestUpdateUnsetManyToManyListAsync: Relationship count is not correct for Object with Id {objectA.Id}");

                foreach (var objectBKey in storedChildKeyList)
                {
                    if (!expectedChildIds.Contains(objectBKey))
                        return new Tuple<bool, string>(false, "TestUpdateUnsetManyToManyListAsync: Relationship ID is not correct");
                }
            }

            return new Tuple<bool, string>(true, "TestUpdateUnsetManyToManyListAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetManyToManyListAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestManyToManyCircularAsync()
    {
        try
        {
            // In this test we will create a many-to-many relationship between instances of the same class
            // including inverse relationship

            // This is the hierarchy that we're going to implement
            //                      1
            //                     / \
            //                   [2] [3]
            //                  /  \ /  \
            //                 4    5    6

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassG>();
            await conn.DropTableAsync<ClassGClassG>();
            await conn.CreateTableAsync<M2MClassG>();
            await conn.CreateTableAsync<ClassGClassG>();

            var object1 = new M2MClassG { Name = "Object 1" };
            var object2 = new M2MClassG { Name = "Object 2" };
            var object3 = new M2MClassG { Name = "Object 3" };
            var object4 = new M2MClassG { Name = "Object 4" };
            var object5 = new M2MClassG { Name = "Object 5" };
            var object6 = new M2MClassG { Name = "Object 6" };

            var objects = new List<M2MClassG> { object1, object2, object3, object4, object5, object6 };
            await conn.InsertAllAsync(objects);

            object2.Parents = [object1];
            object2.Children = [object4, object5];
            await conn.UpdateWithChildrenAsync(object2);

            object3.Parents = [object1];
            object3.Children = [object5, object6];
            await conn.UpdateWithChildrenAsync(object3);

            // These relationships are discovered at runtime
            object1.Children = [object2, object3];
            object4.Parents = [object2];
            object5.Parents = [object2, object3];
            object6.Parents = [object3];

            foreach (var expected in objects)
            {
                var obtained = await conn.GetWithChildrenAsync<M2MClassG>(expected.Id);

                // Check Name
                if (expected.Name != obtained.Name)
                    return new Tuple<bool, string>(false, $"TestManyToManyCircularAsync failed for object with Id {expected.Id}: Name mismatch");

                // Check Children count
                if ((expected.Children ?? []).Count != (obtained.Children ?? []).Count)
                    return new Tuple<bool, string>(false, $"TestManyToManyCircularAsync failed for object with Id {expected.Id}: Children count mismatch");

                // Check Parents count
                if ((expected.Parents ?? []).Count != (obtained.Parents ?? []).Count)
                    return new Tuple<bool, string>(false, $"TestManyToManyCircularAsync failed for object with Id {expected.Id}: Parents count mismatch");

                // Check each child
                foreach (var child in expected.Children ?? Enumerable.Empty<M2MClassG>())
                {
                    if (!obtained.Children.Any(c => c.Id == child.Id && c.Name == child.Name))
                        return new Tuple<bool, string>(false, $"TestManyToManyCircularAsync failed for object with Id {expected.Id}: Child with Id {child.Id} not found");
                }

                // Check each parent
                foreach (var parent in expected.Parents ?? Enumerable.Empty<M2MClassG>())
                {
                    if (!obtained.Parents.Any(p => p.Id == parent.Id && p.Name == parent.Name))
                        return new Tuple<bool, string>(false, $"TestManyToManyCircularAsync failed for object with Id {expected.Id}: Parent with Id {parent.Id} not found");
                }
            }

            return new Tuple<bool, string>(true, "TestManyToManyCircularAsync passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToManyCircularAsync failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestGetManyToManyGuidIdentifierAsync()
    {
        try
        {
            // In this test we will create a N:M relationship between objects of ClassE and ClassF
            //      Class E     -       Class F
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassE>();
            await conn.DropTableAsync<M2MClassF>();
            await conn.DropTableAsync<ClassEClassF>();
            await conn.CreateTableAsync<M2MClassE>();
            await conn.CreateTableAsync<M2MClassF>();
            await conn.CreateTableAsync<ClassEClassF>();

            // Use standard SQLite-Net API to create the objects
            var objectsF = new List<M2MClassF>
            {
                new () { Foo = string.Format("1- Foo String {0}", new Random().Next(100)) },
                new () { Foo = string.Format("2- Foo String {0}", new Random().Next(100)) },
                new () { Foo = string.Format("3- Foo String {0}", new Random().Next(100)) },
                new () { Foo = string.Format("4- Foo String {0}", new Random().Next(100)) }
            };
            await conn.InsertAllAsync(objectsF);

            var objectsE = new List<M2MClassE>
            {
                new () { Id = Guid.NewGuid(), Bar = string.Format("1- Bar String {0}", new Random().Next(100)) },
                new () { Id = Guid.NewGuid(), Bar = string.Format("2- Bar String {0}", new Random().Next(100)) },
                new () { Id = Guid.NewGuid(), Bar = string.Format("3- Bar String {0}", new Random().Next(100)) },
                new () { Id = Guid.NewGuid(), Bar = string.Format("4- Bar String {0}", new Random().Next(100)) }
            };

            await conn.InsertAllAsync(objectsE);

            foreach (var objectE in objectsE)
            {
                var copyE = objectE;
                if (objectE.FObjects != null)
                    return new Tuple<bool, string>(false, "TestGetManyToManyGuidIdentifier: FObjects should be null initially");

                // Fetch (yet empty) the relationship
                await conn.GetChildrenAsync(copyE);

                if (copyE.FObjects == null || copyE.FObjects.Length != 0)
                    return new Tuple<bool, string>(false, "TestGetManyToManyGuidIdentifier: FObjects should be empty after GetChildren");
            }

            // Create the relationships in the intermediate table
            for (var eIndex = 0; eIndex < objectsE.Count; eIndex++)
            {
                for (var fIndex = 0; fIndex <= eIndex; fIndex++)
                {
                    await conn.InsertAsync(new ClassEClassF
                    {
                        ClassEId = objectsE[eIndex].Id,
                        ClassFId = objectsF[fIndex].Id
                    });
                }
            }

            // Check the relationships
            for (var i = 0; i < objectsE.Count; i++)
            {
                var objectE = objectsE[i];

                // Relationship still empty because hasn't been refreshed
                if (objectE.FObjects == null || objectE.FObjects.Length != 0)
                    return new Tuple<bool, string>(false, $"TestGetManyToManyGuidIdentifier: FObjects should still be empty for ClassE Id {objectE.Id}");

                // Fetch the relationship
                await conn.GetChildrenAsync(objectE);

                var childrenCount = i + 1;

                if (objectE.FObjects == null || objectE.FObjects.Length != childrenCount)
                    return new Tuple<bool, string>(false, $"TestGetManyToManyGuidIdentifier: Incorrect number of FObjects for ClassE Id {objectE.Id}");

                // Check that the expected "Foo" values match the fetched ones
                var foos = objectsF.GetRange(0, childrenCount).Select(objectF => objectF.Foo).ToList();
                foreach (var objectF in objectE.FObjects)
                {
                    if (!foos.Contains(objectF.Foo))
                        return new Tuple<bool, string>(false, $"TestGetManyToManyGuidIdentifier: Foo value mismatch for ClassE Id {objectE.Id}");
                }
            }

            return new Tuple<bool, string>(true, "TestGetManyToManyGuidIdentifier: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetManyToManyGuidIdentifier: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestManyToManyCircularReadOnlyAsync()
    {
        try
        {
            // In this test we will create a many-to-many relationship between instances of the same class
            // including inverse relationship

            // This is the hierarchy that we're going to implement
            //                     [1]
            //                     / \
            //                   [2] [3]
            //                  /  \ /  \
            //                 4    5    6

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2MClassH>();
            await conn.DropTableAsync<ClassHClassH>();
            await conn.CreateTableAsync<M2MClassH>();
            await conn.CreateTableAsync<ClassHClassH>();

            var object1 = new M2MClassH { Name = "Object 1" };
            var object2 = new M2MClassH { Name = "Object 2" };
            var object3 = new M2MClassH { Name = "Object 3" };
            var object4 = new M2MClassH { Name = "Object 4" };
            var object5 = new M2MClassH { Name = "Object 5" };
            var object6 = new M2MClassH { Name = "Object 6" };

            var objects = new List<M2MClassH> { object1, object2, object3, object4, object5, object6 };
            await conn.InsertAllAsync(objects);

            object1.Children = [object2, object3];
            await conn.UpdateWithChildrenAsync(object1);

            object2.Children = [object4, object5];
            await conn.UpdateWithChildrenAsync(object2);

            object3.Children = [object5, object6];
            await conn.UpdateWithChildrenAsync(object3);

            // These relationships are discovered at runtime
            object2.Parents = [object1];
            object3.Parents = [object1];
            object4.Parents = [object2];
            object5.Parents = [object2, object3];
            object6.Parents = [object3];

            foreach (var expected in objects)
            {
                var obtained = await conn.GetWithChildrenAsync<M2MClassH>(expected.Id);

                // Check Name
                if (expected.Name != obtained.Name)
                    return new Tuple<bool, string>(false, $"TestManyToManyCircularReadOnlyAsync failed for object with Id {expected.Id}: Name mismatch");

                // Check Children count
                if ((expected.Children ?? []).Count != (obtained.Children ?? []).Count)
                    return new Tuple<bool, string>(false, $"TestManyToManyCircularReadOnlyAsync failed for object with Id {expected.Id}: Children count mismatch");

                // Check Parents count
                if ((expected.Parents ?? []).Count != (obtained.Parents ?? []).Count)
                    return new Tuple<bool, string>(false, $"TestManyToManyCircularReadOnlyAsync failed for object with Id {expected.Id}: Parents count mismatch");

                // Check each child
                foreach (var child in expected.Children ?? Enumerable.Empty<M2MClassH>())
                {
                    if (!obtained.Children.Any(c => c.Id == child.Id && c.Name == child.Name))
                        return new Tuple<bool, string>(false, $"TestManyToManyCircularReadOnlyAsync failed for object with Id {expected.Id}: Child with Id {child.Id} not found");
                }

                // Check each parent
                foreach (var parent in expected.Parents ?? Enumerable.Empty<M2MClassH>())
                {
                    if (!obtained.Parents.Any(p => p.Id == parent.Id && p.Name == parent.Name))
                        return new Tuple<bool, string>(false, $"TestManyToManyCircularReadOnlyAsync failed for object with Id {expected.Id}: Parent with Id {parent.Id} not found");
                }
            }

            return new Tuple<bool, string>(true, "TestManyToManyCircularReadOnlyAsync passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToManyCircularReadOnlyAsync failed with exception: {ex.Message}");
        }
    }
}
