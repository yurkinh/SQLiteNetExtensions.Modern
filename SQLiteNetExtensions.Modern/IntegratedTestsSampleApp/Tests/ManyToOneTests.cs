using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace IntegratedTestsSampleApp.Tests;

public class ManyToOneTests
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

    public static Tuple<bool, string> TestGetManyToOne()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<M2OClassA>();
            conn.DropTable<M2OClassB>();
            conn.CreateTable<M2OClassA>();
            conn.CreateTable<M2OClassB>();

            var objectB = new M2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            conn.Insert(objectB);

            var objectA = new M2OClassA();
            conn.Insert(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetManyToOne: Initially OneClassB should be null");

            conn.GetChildren(objectA);
            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetManyToOne: After GetChildren, OneClassB should still be null");

            objectA.OneClassBKey = objectB.Id;

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestGetManyToOne: After setting OneClassBKey, OneClassB should still be null");

            conn.GetChildren(objectA);

            if (objectA.OneClassB == null)
                return new Tuple<bool, string>(false, "TestGetManyToOne: OneClassB should not be null after GetChildren");

            if (objectA.OneClassB.Id != objectB.Id)
                return new Tuple<bool, string>(false, "TestGetManyToOne: OneClassB Id does not match");

            if (objectA.OneClassB.Foo != objectB.Foo)
                return new Tuple<bool, string>(false, "TestGetManyToOne: OneClassB Foo does not match");

            return new Tuple<bool, string>(true, "TestGetManyToOne: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestGetManyToOne: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateSetManyToOne()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<M2OClassA>();
            conn.DropTable<M2OClassB>();
            conn.CreateTable<M2OClassA>();
            conn.CreateTable<M2OClassB>();

            var objectB = new M2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            conn.Insert(objectB);

            var objectA = new M2OClassA();
            conn.Insert(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOne: Initially OneClassB should be null");

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOne: Initially OneClassBKey should be 0");

            objectA.OneClassB = objectB;

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOne: OneClassBKey should still be 0 after setting OneClassB");

            conn.UpdateWithChildren(objectA);

            if (objectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOne: OneClassBKey should match objectB Id after update");

            var newObjectA = conn.Get<M2OClassA>(objectA.Id);

            if (newObjectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateSetManyToOne: OneClassBKey in newObjectA should match objectB Id");

            return new Tuple<bool, string>(true, "TestUpdateSetManyToOne: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateSetManyToOne: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestUpdateUnsetManyToOne()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<M2OClassA>();
            conn.DropTable<M2OClassB>();
            conn.CreateTable<M2OClassA>();
            conn.CreateTable<M2OClassB>();

            var objectB = new M2OClassB
            {
                Foo = string.Format("Foo String {0}", new Random().Next(100))
            };
            conn.Insert(objectB);

            var objectA = new M2OClassA();
            conn.Insert(objectA);

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: Initially OneClassB should be null");

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: Initially OneClassBKey should be 0");

            objectA.OneClassB = objectB;

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: OneClassBKey should still be 0 after setting OneClassB");

            conn.UpdateWithChildren(objectA);

            if (objectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: OneClassBKey should match objectB Id after update");

            objectA.OneClassB = null;

            if (objectA.OneClassBKey != objectB.Id)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: OneClassBKey should not have been updated yet after unsetting");

            conn.UpdateWithChildren(objectA);

            if (objectA.OneClassBKey != 0)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: OneClassBKey should be 0 after unsetting the relationship");

            if (objectA.OneClassB != null)
                return new Tuple<bool, string>(false, "TestUpdateUnsetManyToOne: OneClassB should be null after unsetting");

            return new Tuple<bool, string>(true, "TestUpdateUnsetManyToOne: Test passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestUpdateUnsetManyToOne: Test failed with exception: {ex.Message}");
        }
    }
}