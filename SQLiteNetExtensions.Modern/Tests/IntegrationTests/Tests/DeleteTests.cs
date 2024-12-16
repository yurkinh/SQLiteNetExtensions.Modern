using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLiteNetExtensions.Extensions;
using SQLite;

namespace SQLiteNetExtensions.IntegrationTests.Tests
{
    [TestFixture]
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

        [Test]
        public void TestDeleteAllGuidPK()
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
            Assert.AreEqual(elementsList.Count, conn.Table<DummyClassGuidPK>().Count());

            var elementsToDelete = new List<DummyClassGuidPK> { elementA, elementC };

            // Delete elements from the database
            conn.DeleteAll(elementsToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, conn.Table<DummyClassGuidPK>().Count());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(conn.Find<DummyClassGuidPK>(deletedElement.Id));
            }
        }

        [Test]
        public void TestDeleteAllIntPK()
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
            Assert.AreEqual(elementsList.Count, conn.Table<DummyClassIntPK>().Count());

            var elementsToDelete = new List<DummyClassIntPK> { elementA, elementC };

            // Delete elements from the database
            conn.DeleteAll(elementsToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, conn.Table<DummyClassIntPK>().Count());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(conn.Find<DummyClassIntPK>(deletedElement.Id));
            }
        }

        [Test]
        public void TestDeleteAllThousandObjects()
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
            Assert.AreEqual(elementsList.Count, conn.Table<DummyClassIntPK>().Count());

            var elementsToDelete = new List<DummyClassIntPK>(elementsList);
            elementsToDelete.RemoveAt(0);

            // Delete elements from the database
            conn.DeleteAll(elementsToDelete, true);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, conn.Table<DummyClassIntPK>().Count());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(conn.Find<DummyClassIntPK>(deletedElement.Id));
            }
        }

        [Test]
        public void TestDeleteAllIdsGuidPK()
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
            Assert.AreEqual(elementsList.Count, conn.Table<DummyClassGuidPK>().Count());

            var elementsToDelete = new List<DummyClassGuidPK> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            conn.DeleteAllIds<DummyClassGuidPK>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, conn.Table<DummyClassGuidPK>().Count());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(conn.Find<DummyClassGuidPK>(deletedElement.Id));
            }
        }

        [Test]
        public void TestDeleteAllIdsIntPK()
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
            Assert.AreEqual(elementsList.Count, conn.Table<DummyClassIntPK>().Count());

            var elementsToDelete = new List<DummyClassIntPK> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            conn.DeleteAllIds<DummyClassIntPK>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, conn.Table<DummyClassIntPK>().Count());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(conn.Find<DummyClassIntPK>(deletedElement.Id));
            }
        }
    }
}

