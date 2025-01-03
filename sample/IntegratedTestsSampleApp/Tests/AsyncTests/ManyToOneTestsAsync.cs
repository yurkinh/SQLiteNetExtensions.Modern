using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace IntegratedTestsSampleApp.Tests;

public class ManyToOneTestsAsync
{
    public class M2OClassA
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(M2OClassB))]
        public int OneClassBKey { get; set; }

        [ManyToOne]
        public M2OClassB OneClassB { get; set; }
    }

    [Table("m2o_class_b")]
    public class M2OClassB
    {
        [PrimaryKey, AutoIncrement, Column("_id_")]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    public static async Task<Tuple<bool, string>> TestGetManyToOneAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2OClassA>();
            await conn.DropTableAsync<M2OClassB>();
            await conn.CreateTableAsync<M2OClassA>();
            await conn.CreateTableAsync<M2OClassB>();

            var objectB = new M2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectB);

            var objectA = new M2OClassA();
            await conn.InsertAsync(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetManyToOneAsync: Initially OneClassB should be null");

            await conn.GetChildrenAsync(objectA);
            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetManyToOneAsync: After GetChildren, OneClassB should still be null");

            objectA.OneClassBKey = objectB.Id;

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetManyToOneAsync: After setting OneClassBKey, OneClassB should still be null");

            await conn.GetChildrenAsync(objectA);

            if (objectA.OneClassB == null)
                return new Tuple<bool, string>(false, "TestGetManyToOneAsync: OneClassB should not be null after GetChildren");

            if (objectA.OneClassB.Id != objectB.Id)
                return new Tuple<bool, string>(false, "TestGetManyToOneAsync: OneClassB Id does not match");

            if (objectA.OneClassB.Foo != objectB.Foo)
                return new Tuple<bool, string>(false, "TestGetManyToOneAsync: OneClassB Foo does not match");

            return new Tuple<bool, string>(true, "TestGetManyToOneAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetManyToOneAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateSetManyToOneAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2OClassA>();
            await conn.DropTableAsync<M2OClassB>();
            await conn.CreateTableAsync<M2OClassA>();
            await conn.CreateTableAsync<M2OClassB>();

            var objectB = new M2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectB);

            var objectA = new M2OClassA();
            await conn.InsertAsync(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOneAsync: Initially OneClassB should be null");

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOneAsync: Initially OneClassBKey should be 0");

            objectA.OneClassB = objectB;

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOneAsync: OneClassBKey should still be 0 after setting OneClassB");

            await conn.UpdateWithChildrenAsync(objectA);

            if (objectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOneAsync: OneClassBKey should match objectB Id after update");

            var newObjectA = await conn.GetAsync<M2OClassA>(objectA.Id);

            if (newObjectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOneAsync: OneClassBKey in newObjectA should match objectB Id");

            return new Tuple<bool, string>(true, "TestUpdateSetManyToOneAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetManyToOneAsync: Test failed with exception: {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestUpdateUnsetManyToOneAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<M2OClassA>();
            await conn.DropTableAsync<M2OClassB>();
            await conn.CreateTableAsync<M2OClassA>();
            await conn.CreateTableAsync<M2OClassB>();

            var objectB = new M2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            await conn.InsertAsync(objectB);

            var objectA = new M2OClassA();
            await conn.InsertAsync(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: Initially OneClassB should be null");

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: Initially OneClassBKey should be 0");

            objectA.OneClassB = objectB;

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: OneClassBKey should still be 0 after setting OneClassB");

            await conn.UpdateWithChildrenAsync(objectA);

            if (objectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: OneClassBKey should match objectB Id after update");

            objectA.OneClassB = null;

            if (objectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: OneClassBKey should not have been updated yet after unsetting");

            await conn.UpdateWithChildrenAsync(objectA);

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: OneClassBKey should be 0 after unsetting the relationship");

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOneAsync: OneClassB should be null after unsetting");

            return new Tuple<bool, string>(true, "TestUpdateUnsetManyToOneAsync: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetManyToOneAsync: Test failed with exception: {ex.Message}");
        }
    }

}