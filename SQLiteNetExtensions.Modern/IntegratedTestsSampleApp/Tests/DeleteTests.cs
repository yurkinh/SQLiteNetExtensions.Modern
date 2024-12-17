using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;

namespace IntegratedTestsSampleApp.Tests;

public class DeleteTests
{
    public class DummyClassGuidPK
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class DummyClassIntPK
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public static Tuple<bool, string> TestDeleteAllGuidPK()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAll extension method

            var conn = Utils.CreateConnection();
            conn.DropTable<DummyClassGuidPK>();
            conn.CreateTable<DummyClassGuidPK>();

            var elementA = new DummyClassGuidPK
            {
                Id = Guid.NewGuid(),
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassGuidPK
            {
                Id = Guid.NewGuid(),
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassGuidPK
            {
                Id = Guid.NewGuid(),
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassGuidPK> { elementA, elementB, elementC };
            conn.InsertAll(elementsList);

            // Verify that the elements have been inserted correctly
            if (conn.Table<DummyClassGuidPK>().Count() != elementsList.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllGuidPK: Failed at inserting elements");

            var elementsToDelete = new List<DummyClassGuidPK> { elementA, elementC };

            // Delete elements from the database
            conn.DeleteAll(elementsToDelete);

            // Verify that the elements have been deleted correctly
            if (conn.Table<DummyClassGuidPK>().Count() != elementsList.Count - elementsToDelete.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllGuidPK: Failed at deleting elements");

            foreach (var deletedElement in elementsToDelete)
            {
                if (conn.Find<DummyClassGuidPK>(deletedElement.Id) != null)
                    return new Tuple<bool, string>(false, $"TestDeleteAllGuidPK: Element {deletedElement.Id} was not deleted");
            }

            return new Tuple<bool, string>(true, "TestDeleteAllGuidPK: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestDeleteAllGuidPK: Exception occurred");
        }
    }

    public static Tuple<bool, string> TestDeleteAllIntPK()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAll extension method

            var conn = Utils.CreateConnection();
            conn.DropTable<DummyClassIntPK>();
            conn.CreateTable<DummyClassIntPK>();

            var elementA = new DummyClassIntPK
            {
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassIntPK
            {
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassIntPK
            {
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassIntPK> { elementA, elementB, elementC };
            conn.InsertAll(elementsList);

            // Verify that the elements have been inserted correctly
            if (conn.Table<DummyClassIntPK>().Count() != elementsList.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllIntPK: Failed at inserting elements");

            var elementsToDelete = new List<DummyClassIntPK> { elementA, elementC };

            // Delete elements from the database
            conn.DeleteAll(elementsToDelete);

            // Verify that the elements have been deleted correctly
            if (conn.Table<DummyClassIntPK>().Count() != elementsList.Count - elementsToDelete.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllIntPK: Failed at deleting elements");

            foreach (var deletedElement in elementsToDelete)
            {
                if (conn.Find<DummyClassIntPK>(deletedElement.Id) != null)
                    return new Tuple<bool, string>(false, $"TestDeleteAllIntPK: Element {deletedElement.Id} was not deleted");
            }

            return new Tuple<bool, string>(true, "TestDeleteAllIntPK: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestDeleteAllIntPK: Exception occurred");
        }
    }

    public static Tuple<bool, string> TestDeleteAllThousandObjects()
    {
        try
        {
            // In this test we will create thousands of elements in the database all but one with
            // the DeleteAll method

            var conn = Utils.CreateConnection();
            conn.DropTable<DummyClassIntPK>();
            conn.CreateTable<DummyClassIntPK>();

            var elementsList = Enumerable.Range(0, 10000).Select(i =>
                new DummyClassIntPK
                {
                    Foo = "Foo " + i,
                    Bar = "Bar " + i
                }
            ).ToList();

            conn.InsertAll(elementsList);

            // Verify that the elements have been inserted correctly
            if (conn.Table<DummyClassIntPK>().Count() != elementsList.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllThousandObjects: Failed at inserting elements");

            var elementsToDelete = new List<DummyClassIntPK>(elementsList);
            elementsToDelete.RemoveAt(0);

            // Delete elements from the database
            conn.DeleteAll(elementsToDelete, true);

            // Verify that the elements have been deleted correctly
            if (conn.Table<DummyClassIntPK>().Count() != elementsList.Count - elementsToDelete.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllThousandObjects: Failed at deleting elements");

            foreach (var deletedElement in elementsToDelete)
            {
                if (conn.Find<DummyClassIntPK>(deletedElement.Id) != null)
                    return new Tuple<bool, string>(false, $"TestDeleteAllThousandObjects: Element {deletedElement.Id} was not deleted");
            }

            return new Tuple<bool, string>(true, "TestDeleteAllThousandObjects: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestDeleteAllThousandObjects: Exception occurred");
        }
    }

    public static Tuple<bool, string> TestDeleteAllIdsGuidPK()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAllIds extension method

            var conn = Utils.CreateConnection();
            conn.DropTable<DummyClassGuidPK>();
            conn.CreateTable<DummyClassGuidPK>();

            var elementA = new DummyClassGuidPK
            {
                Id = Guid.NewGuid(),
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassGuidPK
            {
                Id = Guid.NewGuid(),
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassGuidPK
            {
                Id = Guid.NewGuid(),
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassGuidPK> { elementA, elementB, elementC };
            conn.InsertAll(elementsList);

            // Verify that the elements have been inserted correctly
            if (conn.Table<DummyClassGuidPK>().Count() != elementsList.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllIdsGuidPK: Failed at inserting elements");

            var elementsToDelete = new List<DummyClassGuidPK> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            conn.DeleteAllIds<DummyClassGuidPK>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            if (conn.Table<DummyClassGuidPK>().Count() != elementsList.Count - elementsToDelete.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllIdsGuidPK: Failed at deleting elements");

            foreach (var deletedElement in elementsToDelete)
            {
                if (conn.Find<DummyClassGuidPK>(deletedElement.Id) != null)
                    return new Tuple<bool, string>(false, $"TestDeleteAllIdsGuidPK: Element {deletedElement.Id} was not deleted");
            }

            return new Tuple<bool, string>(true, "TestDeleteAllIdsGuidPK: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestDeleteAllIdsGuidPK: Exception occurred");
        }
    }

    public static Tuple<bool, string> TestDeleteAllIdsIntPK()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAllIds extension method

            var conn = Utils.CreateConnection();
            conn.DropTable<DummyClassIntPK>();
            conn.CreateTable<DummyClassIntPK>();

            var elementA = new DummyClassIntPK
            {
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassIntPK
            {
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassIntPK
            {
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassIntPK> { elementA, elementB, elementC };
            conn.InsertAll(elementsList);

            // Verify that the elements have been inserted correctly
            if (conn.Table<DummyClassIntPK>().Count() != elementsList.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllIdsIntPK: Failed at inserting elements");

            var elementsToDelete = new List<DummyClassIntPK> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            conn.DeleteAllIds<DummyClassIntPK>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            if (conn.Table<DummyClassIntPK>().Count() != elementsList.Count - elementsToDelete.Count)
                return new Tuple<bool, string>(false, "TestDeleteAllIdsIntPK: Failed at deleting elements");

            foreach (var deletedElement in elementsToDelete)
            {
                if (conn.Find<DummyClassIntPK>(deletedElement.Id) != null)
                    return new Tuple<bool, string>(false, $"TestDeleteAllIdsIntPK: Element {deletedElement.Id} was not deleted");
            }

            return new Tuple<bool, string>(true, "TestDeleteAllIdsIntPK: Passed");
        }
        catch
        {
            return new Tuple<bool, string>(false, "TestDeleteAllIdsIntPK: Exception occurred");
        }
    }
}
