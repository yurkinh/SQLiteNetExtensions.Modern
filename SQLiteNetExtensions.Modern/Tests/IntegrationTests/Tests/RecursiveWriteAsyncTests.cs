using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;

#if USING_MVVMCROSS
using SQLite.Net.Attributes;
#else
using SQLite;
#endif

namespace SQLiteNetExtensions.IntegrationTests.Tests
{
    [TestFixture]
    public class RecursiveWriteAsyncTests
    {
        #region OneToOneRecursiveInsertAsync
        public class Person
        {
            [PrimaryKey, AutoIncrement]
            public int Identifier { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public Passport Passport { get; set; }
        }

        public class Passport
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string PassportNumber { get; set; }

            [ForeignKey(typeof(Person))]
            public int OwnerId { get; set; }

            [OneToOne(ReadOnly = true)]
            public Person Owner { get; set; }
        }

        [Test]
        public async void TestOneToOneRecursiveInsertAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Passport>();
            await conn.DropTableAsync<Person>();
            await conn.CreateTableAsync<Passport>();
            await conn.CreateTableAsync<Person>();

            var person = new Person
            {
                Name = "John",
                Surname = "Smith",
                Passport = new Passport
                {
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertWithChildrenAsync(person, recursive: true);

            var obtainedPerson = await conn.FindAsync<Person>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<Passport>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }

        [Test]
        public async void TestOneToOneRecursiveInsertOrReplaceAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Passport>();
            await conn.DropTableAsync<Person>();
            await conn.CreateTableAsync<Passport>();
            await conn.CreateTableAsync<Person>();

            var person = new Person
            {
                Name = "John",
                Surname = "Smith",
                Passport = new Passport
                {
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, recursive: true);

            var obtainedPerson = await conn.FindAsync<Person>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<Passport>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));


            var newPerson = new Person
            {
                Identifier = person.Identifier,
                Name = "John",
                Surname = "Smith",
                Passport = new Passport
                {
                    Id = person.Passport.Id,
                    PassportNumber = "JS123456"
                }
            };
            person = newPerson;

            // Replace the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, recursive: true);

            obtainedPerson = await conn.FindAsync<Person>(person.Identifier);
            obtainedPassport = await conn.FindAsync<Passport>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }
        #endregion

        #region OneToOneRecursiveInsertGuidAsync
        public class PersonGuid
        {
            [PrimaryKey]
            public Guid Identifier { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public PassportGuid Passport { get; set; }
        }

        public class PassportGuid
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string PassportNumber { get; set; }

            [ForeignKey(typeof(PersonGuid))]
            public Guid OwnerId { get; set; }

            [OneToOne(ReadOnly = true)]
            public PersonGuid Owner { get; set; }
        }

        [Test]
        public async void TestOneToOneRecursiveInsertGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportGuid>();
            await conn.DropTableAsync<PersonGuid>();
            await conn.CreateTableAsync<PassportGuid>();
            await conn.CreateTableAsync<PersonGuid>();

            var person = new PersonGuid
            {
                Identifier = Guid.NewGuid(),
                Name = "John",
                Surname = "Smith",
                Passport = new PassportGuid
                {
                    Id = Guid.NewGuid(),
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertWithChildrenAsync(person, recursive: true);

            var obtainedPerson = await conn.FindAsync<PersonGuid>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }

        [Test]
        public async void TestOneToOneRecursiveInsertOrReplaceGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportGuid>();
            await conn.DropTableAsync<PersonGuid>();
            await conn.CreateTableAsync<PassportGuid>();
            await conn.CreateTableAsync<PersonGuid>();

            var person = new PersonGuid
            {
                Identifier = Guid.NewGuid(),
                Name = "John",
                Surname = "Smith",
                Passport = new PassportGuid
                {
                    Id = Guid.NewGuid(),
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, recursive: true);

            var obtainedPerson = await conn.FindAsync<PersonGuid>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));


            var newPerson = new PersonGuid
            {
                Identifier = person.Identifier,
                Name = "John",
                Surname = "Smith",
                Passport = new PassportGuid
                {
                    Id = person.Passport.Id,
                    PassportNumber = "JS123456"
                }
            };
            person = newPerson;

            // Replace the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, recursive: true);

            obtainedPerson = await conn.FindAsync<PersonGuid>(person.Identifier);
            obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }
        #endregion

        #region OneToManyRecursiveInsertAsync
        public class Customer
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
            public Order[] Orders { get; set; }
        }

        [Table("Orders")] // 'Order' is a reserved keyword
        public class Order
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public float Amount { get; set; }
            public DateTime Date { get; set; }

            [ForeignKey(typeof(Customer))]
            public int CustomerId { get; set; }

            [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public Customer Customer { get; set; }
        }

        [Test]
        public async void TestOneToManyRecursiveInsertAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertWithChildrenAsync(customer, recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        [Test]
        public async void TestOneToManyRecursiveInsertOrReplaceAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertOrReplaceWithChildrenAsync(customer);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }

            var newCustomer = new Customer
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new Order { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                    new Order { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                    new Order { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new Order { Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new Order { Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            await conn.InsertOrReplaceWithChildrenAsync(customer, recursive: true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }
        #endregion

        #region OneToManyRecursiveInsertGuidAsync
        public class CustomerGuid
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
            public OrderGuid[] Orders { get; set; }
        }

        [Table("Orders")] // 'Order' is a reserved keyword
        public class OrderGuid
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public float Amount { get; set; }
            public DateTime Date { get; set; }

            [ForeignKey(typeof(CustomerGuid))]
            public Guid CustomerId { get; set; }

            [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public CustomerGuid Customer { get; set; }
        }

        [Test]
        public async void TestOneToManyRecursiveInsertGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertWithChildrenAsync(customer, recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        [Test]
        public async void TestOneToManyRecursiveInsertOrReplaceGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertOrReplaceWithChildrenAsync(customer, recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }

            var newCustomer = new CustomerGuid
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            await conn.InsertOrReplaceWithChildrenAsync(customer, recursive: true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }
        #endregion

        #region ManyToOneRecursiveInsertAsync
        /// <summary>
        /// This test will validate the same scenario than TestOneToManyRecursiveInsert but inserting
        /// one of the orders instead of the customer
        /// </summary>
        [Test]
        public async void TestManyToOneRecursiveInsertAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer;
            await conn.InsertWithChildrenAsync(customer.Orders[0], recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        /// <summary>
        /// This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplace but inserting
        /// one of the orders instead of the customer
        /// </summary>
        [Test]
        public async void TestManyToOneRecursiveInsertOrReplaceAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer;
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }

            var newCustomer = new Customer
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new Order { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                    new Order { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                    new Order { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new Order { Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new Order { Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], recursive: true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }
        #endregion

        #region ManyToOneRecursiveInsertGuidAsync
        /// <summary>
        /// This test will validate the same scenario than TestOneToManyRecursiveInsertGuid but inserting
        /// one of the orders instead of the customer
        /// </summary>
        [Test]
        public async void TestManyToOneRecursiveInsertGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertWithChildrenAsync(customer.Orders[0], recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        /// <summary>
        /// This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplaceGuid but inserting
        /// one of the orders instead of the customer
        /// </summary>
        [Test]
        public async void TestManyToOneRecursiveInsertOrReplaceGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }

            var newCustomer = new CustomerGuid
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], recursive: true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, recursive: true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }
        #endregion

        #region ManyToManyCascadeWithSameClassRelationshipAsync
        public class TwitterUser
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }

            [ManyToMany(typeof(FollowerLeaderRelationshipTable), "LeaderId", "Followers",
                CascadeOperations = CascadeOperation.All)]
            public List<TwitterUser> FollowingUsers { get; set; }

            // ReadOnly is required because we're not specifying the followers manually, but want to obtain them from database
            [ManyToMany(typeof(FollowerLeaderRelationshipTable), "FollowerId", "FollowingUsers",
                CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true)]
            public List<TwitterUser> Followers { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as TwitterUser;
                return other != null && Name.Equals(other.Name);
            }
            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
            public override string ToString()
            {
                return string.Format("[TwitterUser: Id={0}, Name={1}]", Id, Name);
            }
        }

        // Intermediate class, not used directly anywhere in the code, only in ManyToMany attributes and table creation
        public class FollowerLeaderRelationshipTable
        {
            public int LeaderId { get; set; }
            public int FollowerId { get; set; }
        }

        [Test]
        public async void TestManyToManyRecursiveInsertWithSameClassRelationshipAsync()
        {
            // We will configure the following scenario
            // 'John' follows 'Peter' and 'Thomas'
            // 'Thomas' follows 'John'
            // 'Will' follows 'Claire'
            // 'Claire' follows 'Will'
            // 'Jaime' follows 'Peter', 'Thomas' and 'Mark'
            // 'Mark' doesn't follow anyone
            // 'Martha' follows 'Anthony'
            // 'Anthony' follows 'Peter'
            // 'Peter' follows 'Martha'
            //
            // Then, we will insert 'Thomas' and we the other users will be inserted using cascade operations
            //
            // 'Followed by' branches will be ignored in the insert method because the property doesn't have the
            // 'CascadeInsert' operation and it's marked as ReadOnly
            //
            // We'll insert 'Jaime', 'Mark', 'Claire' and 'Will' manually because they're outside the 'Thomas' tree
            //
            // Cascade operations should stop once the user has been inserted once
            // So, more or less, the cascade operation tree will be the following (order may not match)
            // 'Thomas' |-(follows)>  'John' |-(follows)> 'Peter' |-(follows)> 'Martha' |-(follows)> 'Anthony' |-(follows)-> 'Peter'*
            //                               |-(follows)> 'Thomas'*
            //
            //
            // (*) -> Entity already inserted in a previous operation. Stop cascade insert

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<TwitterUser>();
            await conn.DropTableAsync<FollowerLeaderRelationshipTable>();
            await conn.CreateTableAsync<TwitterUser>();
            await conn.CreateTableAsync<FollowerLeaderRelationshipTable>();

            var john = new TwitterUser { Name = "John" };
            var thomas = new TwitterUser { Name = "Thomas" };
            var will = new TwitterUser { Name = "Will" };
            var claire = new TwitterUser { Name = "Claire" };
            var jaime = new TwitterUser { Name = "Jaime" };
            var mark = new TwitterUser { Name = "Mark" };
            var martha = new TwitterUser { Name = "Martha" };
            var anthony = new TwitterUser { Name = "anthony" };
            var peter = new TwitterUser { Name = "Peter" };

            john.FollowingUsers = new List<TwitterUser> { peter, thomas };
            thomas.FollowingUsers = new List<TwitterUser> { john };
            will.FollowingUsers = new List<TwitterUser> { claire };
            claire.FollowingUsers = new List<TwitterUser> { will };
            jaime.FollowingUsers = new List<TwitterUser> { peter, thomas, mark };
            mark.FollowingUsers = new List<TwitterUser>();
            martha.FollowingUsers = new List<TwitterUser> { anthony };
            anthony.FollowingUsers = new List<TwitterUser> { peter };
            peter.FollowingUsers = new List<TwitterUser> { martha };

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };

            // Only need to insert Jaime and Claire, the other users are contained in these trees
            await conn.InsertAllWithChildrenAsync(new[] { jaime, claire }, recursive: true);

            Action<TwitterUser, TwitterUser> checkUser = (expected, obtained) =>
            {
                Assert.NotNull(obtained, "User is null: {0}", expected.Name);
                Assert.AreEqual(expected.Name, obtained.Name);
                Assert.That(obtained.FollowingUsers, Is.EquivalentTo(expected.FollowingUsers));
                var followers = allUsers.Where(u => u.FollowingUsers.Contains(expected));
                Assert.That(obtained.Followers, Is.EquivalentTo(followers));
            };

            var obtainedThomas = await conn.GetWithChildrenAsync<TwitterUser>(thomas.Id, recursive: true);
            checkUser(thomas, obtainedThomas);

            var obtainedJohn = obtainedThomas.FollowingUsers.FirstOrDefault(u => u.Id == john.Id);
            checkUser(john, obtainedJohn);

            var obtainedPeter = obtainedJohn.FollowingUsers.FirstOrDefault(u => u.Id == peter.Id);
            checkUser(peter, obtainedPeter);

            var obtainedMartha = obtainedPeter.FollowingUsers.FirstOrDefault(u => u.Id == martha.Id);
            checkUser(martha, obtainedMartha);

            var obtainedAnthony = obtainedMartha.FollowingUsers.FirstOrDefault(u => u.Id == anthony.Id);
            checkUser(anthony, obtainedAnthony);

            var obtainedJaime = obtainedThomas.Followers.FirstOrDefault(u => u.Id == jaime.Id);
            checkUser(jaime, obtainedJaime);

            var obtainedMark = obtainedJaime.FollowingUsers.FirstOrDefault(u => u.Id == mark.Id);
            checkUser(mark, obtainedMark);

        }

        [Test]
        public async void TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync()
        {
            // We will configure the following scenario
            // 'John' follows 'Peter' and 'Thomas'
            // 'Thomas' follows 'John'
            // 'Will' follows 'Claire'
            // 'Claire' follows 'Will'
            // 'Jaime' follows 'Peter', 'Thomas' and 'Mark'
            // 'Mark' doesn't follow anyone
            // 'Martha' follows 'Anthony'
            // 'Anthony' follows 'Peter'
            // 'Peter' follows 'Martha'
            //
            // Then, we will delete 'Thomas' and the other users will be deleted using cascade operations
            //
            // 'Followed by' branches will be ignored in the delete method because the property doesn't have the
            // 'CascadeDelete' operation and it's marked as ReadOnly
            //
            // 'Jaime', 'Mark', 'Claire' and 'Will' won't be deleted because they're outside the 'Thomas' tree
            //
            // Cascade operations should stop once the user has been marked for deletion once
            // So, more or less, the cascade operation tree will be the following (order may not match)
            // 'Thomas' |-(follows)>  'John' |-(follows)> 'Peter' |-(follows)> 'Martha' |-(follows)> 'Anthony' |-(follows)-> 'Peter'*
            //                               |-(follows)> 'Thomas'*
            //
            //
            // (*) -> Entity already marked for deletion in a previous operation. Stop cascade delete

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<TwitterUser>();
            await conn.DropTableAsync<FollowerLeaderRelationshipTable>();
            await conn.CreateTableAsync<TwitterUser>();
            await conn.CreateTableAsync<FollowerLeaderRelationshipTable>();

            var john = new TwitterUser { Name = "John" };
            var thomas = new TwitterUser { Name = "Thomas" };
            var will = new TwitterUser { Name = "Will" };
            var claire = new TwitterUser { Name = "Claire" };
            var jaime = new TwitterUser { Name = "Jaime" };
            var mark = new TwitterUser { Name = "Mark" };
            var martha = new TwitterUser { Name = "Martha" };
            var anthony = new TwitterUser { Name = "anthony" };
            var peter = new TwitterUser { Name = "Peter" };

            john.FollowingUsers = new List<TwitterUser> { peter, thomas };
            thomas.FollowingUsers = new List<TwitterUser> { john };
            will.FollowingUsers = new List<TwitterUser> { claire };
            claire.FollowingUsers = new List<TwitterUser> { will };
            jaime.FollowingUsers = new List<TwitterUser> { peter, thomas, mark };
            mark.FollowingUsers = new List<TwitterUser>();
            martha.FollowingUsers = new List<TwitterUser> { anthony };
            anthony.FollowingUsers = new List<TwitterUser> { peter };
            peter.FollowingUsers = new List<TwitterUser> { martha };

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };

            // Inserts all the objects in the database recursively
            await conn.InsertAllWithChildrenAsync(allUsers, recursive: true);

            // Deletes the entity tree starting at 'Thomas' recursively
            await conn.DeleteAsync(thomas, recursive: true);

            var expectedUsers = new[] { jaime, mark, claire, will };
            var existingUsers = await conn.Table<TwitterUser>().ToListAsync();

            // Check that the users have been deleted and only the users outside the 'Thomas' tree still exist
            Assert.That(existingUsers, Is.EquivalentTo(expectedUsers));
        }
        #endregion

        #region InsertTextBlobPropertiesRecursiveAsync
        class Teacher
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
            public List<Student> Students { get; set; }
        }

        class Student
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }

            [ManyToOne]
            public Teacher Teacher { get; set; }

            [TextBlob("AddressBlob")]
            public Address Address { get; set; }

            [ForeignKey(typeof(Teacher))]
            public int TeacherId { get; set; }
            public String AddressBlob { get; set; }

        }

        class Address
        {
            public string Street { get; set; }
            public string Town { get; set; }
        }

        [Test]
        public async void TestInsertTextBlobPropertiesRecursiveAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Student>();
            await conn.DropTableAsync<Teacher>();
            await conn.CreateTableAsync<Student>();
            await conn.CreateTableAsync<Teacher>();

            var teacher = new Teacher
            {
                Name = "John Smith",
                Students = new List<Student> {
                    new Student {
                        Name = "Bruce Banner",
                        Address = new Address {
                            Street = "Sesame Street 5",
                            Town = "Gotham City"
                        }
                    },
                    new Student {
                        Name = "Peter Parker",
                        Address = new Address {
                            Street = "Arlington Road 69",
                            Town = "Arkham City"
                        }
                    },
                    new Student {
                        Name = "Steve Rogers",
                        Address = new Address {
                            Street = "28th Street 19",
                            Town = "New York"
                        }
                    }
                }
            };

            await conn.InsertWithChildrenAsync(teacher, recursive: true);

            foreach (var student in teacher.Students)
            {
                var dbStudent = await conn.GetWithChildrenAsync<Student>(student.Id);
                Assert.NotNull(dbStudent);
                Assert.NotNull(dbStudent.Address);
                Assert.AreEqual(student.Address.Street, dbStudent.Address.Street);
                Assert.AreEqual(student.Address.Town, dbStudent.Address.Town);
            }
        }
        #endregion
    }
}

