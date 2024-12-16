using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;

#if USING_MVVMCROSS
#else
using SQLite;
#endif

namespace SQLiteNetExtensions.IntegrationTests.Tests
{
    [TestFixture]
    public class RecursiveReadAsyncTests
    {
        #region TestOneToOneCascadeWithInverseAsync
        public class PassportWithForeignKey
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string PassportNumber { get; set; }

            [ForeignKey(typeof(PersonNoForeignKey))]
            public int OwnerId { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
            public PersonNoForeignKey Owner { get; set; }
        }

        public class PersonNoForeignKey
        {
            [PrimaryKey, AutoIncrement]
            public int Identifier { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
            public PassportWithForeignKey Passport { get; set; }
        }

        [Test]
        public async void TestOneToOneCascadeWithInverseAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportWithForeignKey>();
            await conn.DropTableAsync<PersonNoForeignKey>();
            await conn.CreateTableAsync<PassportWithForeignKey>();
            await conn.CreateTableAsync<PersonNoForeignKey>();

            var person = new PersonNoForeignKey { Name = "John", Surname = "Smith" };
            await conn.InsertAsync(person);

            var passport = new PassportWithForeignKey { PassportNumber = "JS12345678", Owner = person };
            await conn.InsertAsync(passport);
            await conn.UpdateWithChildrenAsync(passport);

            var obtainedPerson = await conn.GetWithChildrenAsync<PersonNoForeignKey>(person.Identifier, recursive: true);
            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPerson.Passport);
            Assert.NotNull(obtainedPerson.Passport.Owner, "Circular reference should've been solved");
            Assert.AreEqual(obtainedPerson.Identifier, obtainedPerson.Passport.Owner.Identifier, "Integral reference check");
            Assert.AreEqual(obtainedPerson.Passport.Id, obtainedPerson.Passport.Owner.Passport.Id);
            Assert.AreEqual(person.Identifier, obtainedPerson.Identifier);
            Assert.AreEqual(passport.Id, obtainedPerson.Passport.Id);

            var obtainedPassport = await conn.GetWithChildrenAsync<PassportWithForeignKey>(passport.Id, recursive: true);
            Assert.NotNull(obtainedPassport);
            Assert.NotNull(obtainedPassport.Owner);
            Assert.NotNull(obtainedPassport.Owner.Passport, "Circular reference should've been solved");
            Assert.AreEqual(obtainedPassport.Id, obtainedPassport.Owner.Passport.Id);
            Assert.AreEqual(obtainedPassport.Owner.Identifier, obtainedPassport.Owner.Passport.Owner.Identifier);
            Assert.AreEqual(passport.Id, obtainedPassport.Id);
            Assert.AreEqual(person.Identifier, obtainedPassport.Owner.Identifier);
        }

        /// <summary>
        /// Same test that TestOneToOneCascadeWithInverse but fetching the passport instead of the person
        /// </summary>
        [Test]
        public async void TestOneToOneCascadeWithInverseReversedAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportWithForeignKey>();
            await conn.DropTableAsync<PersonNoForeignKey>();
            await conn.CreateTableAsync<PassportWithForeignKey>();
            await conn.CreateTableAsync<PersonNoForeignKey>();

            var person = new PersonNoForeignKey { Name = "John", Surname = "Smith" };
            await conn.InsertAsync(person);

            var passport = new PassportWithForeignKey { PassportNumber = "JS12345678", Owner = person };
            await conn.InsertAsync(passport);
            await conn.UpdateWithChildrenAsync(passport);

            var obtainedPassport = await conn.GetWithChildrenAsync<PassportWithForeignKey>(passport.Id, recursive: true);
            Assert.NotNull(obtainedPassport);
            Assert.NotNull(obtainedPassport.Owner);
            Assert.NotNull(obtainedPassport.Owner.Passport, "Circular reference should've been solved");
            Assert.AreEqual(obtainedPassport.Id, obtainedPassport.Owner.Passport.Id);
            Assert.AreEqual(obtainedPassport.Owner.Identifier, obtainedPassport.Owner.Passport.Owner.Identifier);
            Assert.AreEqual(passport.Id, obtainedPassport.Id);
            Assert.AreEqual(person.Identifier, obtainedPassport.Owner.Identifier);
        }
        #endregion

        #region TestOneToOneCascadeWithInverseDoubleForeignKeyAsync
        public class PassportWithForeignKeyDouble
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string PassportNumber { get; set; }

            [ForeignKey(typeof(PersonWithForeignKey))]
            public int OwnerId { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
            public PersonWithForeignKey Owner { get; set; }
        }

        public class PersonWithForeignKey
        {
            [PrimaryKey]
            public int Identifier { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }

            [ForeignKey(typeof(PassportWithForeignKeyDouble))]
            public int PassportId { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
            public PassportWithForeignKeyDouble Passport { get; set; }
        }

        [Test]
        public async void TestOneToOneCascadeWithInverseDoubleForeignKey()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportWithForeignKeyDouble>();
            await conn.DropTableAsync<PersonWithForeignKey>();
            await conn.CreateTableAsync<PassportWithForeignKeyDouble>();
            await conn.CreateTableAsync<PersonWithForeignKey>();

            var person = new PersonWithForeignKey { Name = "John", Surname = "Smith" };
            await conn.InsertAsync(person);

            var passport = new PassportWithForeignKeyDouble { PassportNumber = "JS12345678", Owner = person };
            await conn.InsertAsync(passport);
            await conn.UpdateWithChildrenAsync(passport);

            var obtainedPerson = await conn.GetWithChildrenAsync<PersonWithForeignKey>(person.Identifier, recursive: true);
            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPerson.Passport);
            Assert.NotNull(obtainedPerson.Passport.Owner, "Circular reference should've been solved");
            Assert.AreEqual(obtainedPerson.Identifier, obtainedPerson.Passport.Owner.Identifier, "Integral reference check");
            Assert.AreEqual(obtainedPerson.Passport.Id, obtainedPerson.Passport.Owner.Passport.Id);
            Assert.AreEqual(person.Identifier, obtainedPerson.Identifier);
            Assert.AreEqual(passport.Id, obtainedPerson.Passport.Id);
        }

        /// <summary>
        /// Same test that TestOneToOneCascadeWithInverseDoubleForeignKey but fetching the passport instead of the person
        /// </summary>
        [Test]
        public async void TestOneToOneCascadeWithInverseDoubleForeignKeyReversed()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportWithForeignKeyDouble>();
            await conn.DropTableAsync<PersonWithForeignKey>();
            await conn.CreateTableAsync<PassportWithForeignKeyDouble>();
            await conn.CreateTableAsync<PersonWithForeignKey>();

            var person = new PersonWithForeignKey { Name = "John", Surname = "Smith" };
            await conn.InsertAsync(person);

            var passport = new PassportWithForeignKeyDouble { PassportNumber = "JS12345678", Owner = person };
            await conn.InsertAsync(passport);
            await conn.UpdateWithChildrenAsync(passport);

            var obtainedPassport = await conn.GetWithChildrenAsync<PassportWithForeignKeyDouble>(passport.Id, recursive: true);
            Assert.NotNull(obtainedPassport);
            Assert.NotNull(obtainedPassport.Owner);
            Assert.NotNull(obtainedPassport.Owner.Passport, "Circular reference should've been solved");
            Assert.AreEqual(obtainedPassport.Id, obtainedPassport.Owner.Passport.Id);
            Assert.AreEqual(obtainedPassport.Owner.Identifier, obtainedPassport.Owner.Passport.Owner.Identifier);
            Assert.AreEqual(passport.Id, obtainedPassport.Id);
            Assert.AreEqual(person.Identifier, obtainedPassport.Owner.Identifier);
        }
        #endregion


        #region OneToManyCascadeWithInverseAsync
        public class Customer
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
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

            [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
            public Customer Customer { get; set; }
        }

        [Test]
        public async void TestOneToManyCascadeWithInverseAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer { Name = "John Smith" };
            var orders = new[]
            {
                new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            };

            await conn.InsertAsync(customer);
            await conn.InsertAllAsync(orders);

            customer.Orders = orders;
            await conn.UpdateWithChildrenAsync(customer);

            var expectedOrders = orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

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
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }
        #endregion

        #region ManyToOneCascadeWithInverseAsync
        /// <summary>
        /// In this test we will execute the same test that we did in TestOneToManyCascadeWithInverse but fetching
        /// one of the orders
        /// </summary>
        [Test]
        public async void TestManyToOneCascadeWithInverseAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer { Name = "John Smith" };
            var orders = new[]
            {
                new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            };

            await conn.InsertAsync(customer);
            await conn.InsertAllAsync(orders);

            customer.Orders = orders;
            await conn.UpdateWithChildrenAsync(customer);

            var orderToFetch = orders[2];
            var expectedOrders = orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedOrder = await conn.GetWithChildrenAsync<Order>(orderToFetch.Id, recursive: true);
            Assert.NotNull(obtainedOrder);
            Assert.AreEqual(orderToFetch.Date, obtainedOrder.Date);
            Assert.AreEqual(orderToFetch.Amount, obtainedOrder.Amount, 0.0001);

            var obtainedCustomer = obtainedOrder.Customer;
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
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
                CascadeOperations = CascadeOperation.CascadeRead)]
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
        }

        // Intermediate class, not used directly anywhere in the code, only in ManyToMany attributes and table creation
        public class FollowerLeaderRelationshipTable
        {
            public int LeaderId { get; set; }
            public int FollowerId { get; set; }
        }

        [Test]
        public async void TestManyToManyCascadeWithSameClassRelationshipAsync()
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
            // Then, we will fetch 'Thomas' and we will load the other users using cascade operations
            // 'Claire' and 'Will' won't be loaded because they are outside of the 'Thomas' tree
            //
            // Cascade operations should stop once the user has been loaded once
            // So, more or less, the cascade operation tree will be the following (order may not match)
            // 'Thomas' |-(follows)>  'John' |-(follows)> 'Peter' |-(follows)> 'Martha' |-(follows)> 'Anthony' |-(follows)-> 'Peter'*
            //          |                    |                    |                     |                      |-(followed by)> 'Martha'*
            //          |                    |                    |                     |-(followed by)> 'Peter'*
            //          |                    |                    |-(followed by)> 'John'*
            //          |                    |                    |-(followed by)> 'Jaime'*
            //          |                    |                    |-(followed by)> 'Anthony'*
            //          |                    |-(follows)> 'Thomas'*
            //          |                    |-(followed by)> 'Thomas'*
            //          |
            //          |-(followed by)> 'Jaime' |-(follows)> 'Peter'*
            //          |                        |-(follows)> 'Thomas'*
            //          |                        |-(follows)> 'Mark' |-(followed by)> 'Jaime'*
            //          |-(followed by)> 'John'*
            //
            // (*) -> Entity already loaded in a previous operation. Stop cascade loading

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

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };
            await conn.InsertAllAsync(allUsers);

            john.FollowingUsers = new List<TwitterUser> { peter, thomas };
            thomas.FollowingUsers = new List<TwitterUser> { john };
            will.FollowingUsers = new List<TwitterUser> { claire };
            claire.FollowingUsers = new List<TwitterUser> { will };
            jaime.FollowingUsers = new List<TwitterUser> { peter, thomas, mark };
            mark.FollowingUsers = new List<TwitterUser>();
            martha.FollowingUsers = new List<TwitterUser> { anthony };
            anthony.FollowingUsers = new List<TwitterUser> { peter };
            peter.FollowingUsers = new List<TwitterUser> { martha };

            foreach (var user in allUsers)
            {
                await conn.UpdateWithChildrenAsync(user);
            }

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
                Name = "John Smith"
            };
            await conn.InsertAsync(teacher);

            var students = new List<Student> {
                new Student {
                    Name = "Bruce Banner",
                    Address = new Address {
                        Street = "Sesame Street 5",
                        Town = "Gotham City"
                    },
                    Teacher = teacher
                },
                new Student {
                    Name = "Peter Parker",
                    Address = new Address {
                        Street = "Arlington Road 69",
                        Town = "Arkham City"
                    },
                    Teacher = teacher
                },
                new Student {
                    Name = "Steve Rogers",
                    Address = new Address {
                        Street = "28th Street 19",
                        Town = "New York"
                    },
                    Teacher = teacher
                }
            };
            await conn.InsertAllWithChildrenAsync(students);


            var dbTeacher = await conn.GetWithChildrenAsync<Teacher>(teacher.Id, recursive: true);

            foreach (var student in students)
            {
                var dbStudent = dbTeacher.Students.Find(s => s.Id == student.Id);
                Assert.NotNull(dbStudent);
                Assert.NotNull(dbStudent.Address);
                Assert.AreEqual(student.Address.Street, dbStudent.Address.Street);
                Assert.AreEqual(student.Address.Town, dbStudent.Address.Town);
            }
        }
        #endregion
    }
}

