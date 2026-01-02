using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;

namespace IntegratedTestsSampleApp.Tests;

public static class DeleteTestsAsunc
{
    public class DummyClassGuidPK
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Foo { get; set; } = string.Empty;
        public string Bar { get; set; } = string.Empty;
    }

    public class DummyClassIntPK
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Foo { get; set; } = string.Empty;
        public string Bar { get; set; } = string.Empty;
    }

    public async static Task<Tuple<bool, string>> TestDeleteAllGuidPKAsync()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAll extension method

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<DummyClassGuidPK>();
            await conn.CreateTableAsync<DummyClassGuidPK>();

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
            await conn.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            var ct = await conn.Table<DummyClassGuidPK>().CountAsync();
            if (ct != elementsList.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllGuidPKAsync: Failed at inserting elements");
            }

            var elementsToDelete = new List<DummyClassGuidPK> { elementA, elementC };

            // Delete elements from the database
            await Task.WhenAll(elementsToDelete.Select(element => conn.DeleteAsync(element)));

            // Verify that the elements have been deleted correctly
            if (await conn.Table<DummyClassGuidPK>().CountAsync() != elementsList.Count - elementsToDelete.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllGuidPKAsync: Failed at deleting elements");
            }

            foreach (var deletedElement in elementsToDelete)
            {
                if (await conn.FindAsync<DummyClassGuidPK>(deletedElement.Id) != null)
                {
                    return new Tuple<bool, string>(false, $"TestDeleteAllGuidPKAsync: Element {deletedElement.Id} was not deleted");
                }
            }

            return new Tuple<bool, string>(true, "TestDeleteAllGuidPKAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestDeleteAllGuidPKAsync: Exception occurred - {ex.Message}");
        }
    }

    public async static Task<Tuple<bool, string>> TestDeleteAllIntPKAsync()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAll extension method

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<DummyClassIntPK>();
            await conn.CreateTableAsync<DummyClassIntPK>();

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
            await conn.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            if (await conn.Table<DummyClassIntPK>().CountAsync() != elementsList.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllIntPKAsync: Failed at inserting elements");
            }

            // Delete elements from the database
            var elementsToDelete = new List<DummyClassIntPK> { elementA, elementC };
            await Task.WhenAll(elementsToDelete.Select(element => conn.DeleteAsync(element)));

            // Verify that the elements have been deleted correctly
            if (await conn.Table<DummyClassIntPK>().CountAsync() != elementsList.Count - elementsToDelete.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllIntPKAsync: Failed at deleting elements");
            }

            foreach (var deletedElement in elementsToDelete)
            {
                if (await conn.FindAsync<DummyClassIntPK>(deletedElement.Id) != null)
                {
                    return new Tuple<bool, string>(false, $"TestDeleteAllIntPKAsync: Element {deletedElement.Id} was not deleted");
                }
            }

            return new Tuple<bool, string>(true, "TestDeleteAllIntPKAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestDeleteAllIntPKAsync: Exception occurred - {ex.Message}");
        }
    }

    public async static Task<Tuple<bool, string>> TestDeleteAllThousandObjectsAsync()
    {
        try
        {
            // In this test we will create thousands of elements in the database all but one with
            // the DeleteAll method
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<DummyClassIntPK>();
            await conn.CreateTableAsync<DummyClassIntPK>();

            var elementsList = Enumerable.Range(0, 10000).Select(i =>
                new DummyClassIntPK
                {
                    Foo = "Foo " + i,
                    Bar = "Bar " + i
                }
            ).ToList();

            await conn.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            if (await conn.Table<DummyClassIntPK>().CountAsync() != elementsList.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllThousandObjectsAsync: Failed at inserting elements");
            }

            // Delete elements from the database
            var elementsToDelete = new List<DummyClassIntPK>(elementsList);
            elementsToDelete.RemoveAt(0);

            await Task.WhenAll(elementsToDelete.Select(element => conn.DeleteAsync(element)));

            // Verify that the elements have been deleted correctly
            if (await conn.Table<DummyClassIntPK>().CountAsync() != elementsList.Count - elementsToDelete.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllThousandObjectsAsync: Failed at deleting elements");
            }

            foreach (var deletedElement in elementsToDelete)
            {
                if (await conn.FindAsync<DummyClassIntPK>(deletedElement.Id) != null)
                {
                    return new Tuple<bool, string>(false, $"TestDeleteAllThousandObjectsAsync: Element {deletedElement.Id} was not deleted");
                }
            }

            return new Tuple<bool, string>(true, "TestDeleteAllThousandObjectsAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestDeleteAllThousandObjectsAsync: Exception occurred - {ex.Message}");
        }
    }

    public async static Task<Tuple<bool, string>> TestDeleteAllIdsGuidPKAsync()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAllIds extension method

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<DummyClassGuidPK>();
            await conn.CreateTableAsync<DummyClassGuidPK>();

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
            await conn.InsertAllAsync(elementsList);

            if (await conn.Table<DummyClassGuidPK>().CountAsync() != elementsList.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllIdsGuidPKAsync: Failed at inserting elements");
            }

            var elementsToDelete = new List<DummyClassGuidPK> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            await conn.DeleteAllIdsAsync<DummyClassGuidPK>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            if (await conn.Table<DummyClassGuidPK>().CountAsync() != elementsList.Count - elementsToDelete.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllIdsGuidPKAsync: Failed at deleting elements");
            }

            foreach (var deletedElement in elementsToDelete)
            {
                if (await conn.FindAsync<DummyClassGuidPK>(deletedElement.Id) != null)
                {
                    return new Tuple<bool, string>(false, $"TestDeleteAllIdsGuidPKAsync: Element {deletedElement.Id} was not deleted");
                }
            }

            return new Tuple<bool, string>(true, "TestDeleteAllIdsGuidPKAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestDeleteAllIdsGuidPKAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestDeleteAllIdsIntPKAsync()
    {
        try
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAllIds extension method

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<DummyClassIntPK>();
            await conn.CreateTableAsync<DummyClassIntPK>();

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
            await conn.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            var insertedCount = await conn.Table<DummyClassIntPK>().CountAsync();
            if (insertedCount != elementsList.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllIdsIntPKAsync: Failed at inserting elements");
            }

            var elementsToDelete = new List<DummyClassIntPK> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            await Task.WhenAll(elementsToDelete.Select(element => conn.DeleteAsync(element)));

            // Verify that the elements have been deleted correctly
            var remainingCount = await conn.Table<DummyClassIntPK>().CountAsync();
            if (remainingCount != elementsList.Count - elementsToDelete.Count)
            {
                return new Tuple<bool, string>(false, "TestDeleteAllIdsIntPKAsync: Failed at deleting elements");
            }

            foreach (var deletedElement in elementsToDelete)
            {
                if (await conn.FindAsync<DummyClassIntPK>(deletedElement.Id) != null)
                {
                    return new Tuple<bool, string>(false, $"TestDeleteAllIdsIntPKAsync: Element {deletedElement.Id} was not deleted");
                }
            }

            return new Tuple<bool, string>(true, "TestDeleteAllIdsIntPKAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestDeleteAllIdsIntPKAsync: Exception occurred - {ex.Message}");
        }
    }
}
