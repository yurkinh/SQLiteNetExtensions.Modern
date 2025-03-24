using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using IntegratedTestsSampleApp.Helpers;

namespace IntegratedTestsSampleApp.Tests;

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

    public static async Task<Tuple<bool, string>> TestOneToOneRecursiveInsertAsync()
    {
        try
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
                Passport = new Passport { PassportNumber = "JS123456" }
            };

            // Insert the elements in the database recursively
            await conn.InsertWithChildrenAsync(person, recursive: true);

            var obtainedPerson = await conn.FindAsync<Person>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<Passport>(person.Passport.Id);

            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertAsync: obtainedPerson is null");
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertAsync: obtainedPassport is null");

            if (obtainedPerson.Name != person.Name)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertAsync: Name mismatch");
            if (obtainedPerson.Surname != person.Surname)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertAsync: Surname mismatch");
            if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertAsync: PassportNumber mismatch");
            if (obtainedPassport.OwnerId != person.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertAsync: OwnerId mismatch");

            return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestOneToOneRecursiveInsertOrReplaceAsync()
    {
        try
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
                Passport = new Passport { PassportNumber = "JS123456" }
            };

            // Insert the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, recursive: true);

            var obtainedPerson = await conn.FindAsync<Person>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<Passport>(person.Passport.Id);

            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: obtainedPerson is null");
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: obtainedPassport is null");

            if (obtainedPerson.Name != person.Name)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: Name mismatch");
            if (obtainedPerson.Surname != person.Surname)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: Surname mismatch");
            if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: PassportNumber mismatch");
            if (obtainedPassport.OwnerId != person.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: OwnerId mismatch");

            // Replace the elements in the database recursively
            var newPerson = new Person
            {
                Identifier = person.Identifier,
                Name = "John",
                Surname = "Smith",
                Passport = new Passport { Id = person.Passport.Id, PassportNumber = "JS123456" }
            };
            person = newPerson;

            await conn.InsertOrReplaceWithChildrenAsync(person, recursive: true);

            obtainedPerson = await conn.FindAsync<Person>(person.Identifier);
            obtainedPassport = await conn.FindAsync<Passport>(person.Passport.Id);

            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: obtainedPerson after replace is null");
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: obtainedPassport after replace is null");

            if (obtainedPerson.Name != person.Name)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: Name mismatch after replace");
            if (obtainedPerson.Surname != person.Surname)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: Surname mismatch after replace");
            if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: PassportNumber mismatch after replace");
            if (obtainedPassport.OwnerId != person.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceAsync: OwnerId mismatch after replace");

            return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertOrReplaceAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceAsync: Exception occurred - {ex.Message}");
        }
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

    public static async Task<Tuple<bool, string>> TestOneToOneRecursiveInsertGuidAsync()
    {
        try
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
            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuidAsync: obtainedPerson is null");

            var obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuidAsync: obtainedPassport is null");

            if (obtainedPerson.Name != person.Name)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuidAsync: Name mismatch");
            if (obtainedPerson.Surname != person.Surname)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuidAsync: Surname mismatch");
            if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuidAsync: PassportNumber mismatch");
            if (obtainedPassport.OwnerId != person.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuidAsync: OwnerId mismatch");

            return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertGuidAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertGuidAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestOneToOneRecursiveInsertOrReplaceGuidAsync()
    {
        try
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
            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: obtainedPerson is null");

            var obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: obtainedPassport is null");

            if (obtainedPerson.Name != person.Name)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: Name mismatch");
            if (obtainedPerson.Surname != person.Surname)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: Surname mismatch");
            if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: PassportNumber mismatch");
            if (obtainedPassport.OwnerId != person.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: OwnerId mismatch");

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

            // Replace the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(newPerson, recursive: true);

            obtainedPerson = await conn.FindAsync<PersonGuid>(newPerson.Identifier);
            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: obtainedPerson after replace is null");

            obtainedPassport = await conn.FindAsync<PassportGuid>(newPerson.Passport.Id);
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: obtainedPassport after replace is null");

            if (obtainedPerson.Name != newPerson.Name)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: Name mismatch after replace");
            if (obtainedPerson.Surname != newPerson.Surname)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: Surname mismatch after replace");
            if (obtainedPassport.PassportNumber != newPerson.Passport.PassportNumber)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: PassportNumber mismatch after replace");
            if (obtainedPassport.OwnerId != newPerson.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: OwnerId mismatch after replace");

            return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertOrReplaceGuidAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuidAsync: Exception occurred - {ex.Message}");
        }
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

    public static async Task<Tuple<bool, string>> TestOneToManyRecursiveInsertAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer
            {
                Name = "John Smith",
                Orders = new[] {
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
            if (obtainedCustomer == null || obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertAsync: obtainedCustomer or obtainedCustomer.Orders is null");

            if (expectedOrders.Count != obtainedCustomer.Orders.Length)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertAsync: Order count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id || order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertAsync: Customer mismatch for order {order.Id}");
                if (order.Customer.Orders == null || order.Customer.Orders.Length != expectedOrders.Count)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertAsync: Customer orders count mismatch for order {order.Id}");
            }

            return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestOneToManyRecursiveInsertOrReplaceAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Customer>();
            await conn.DropTableAsync<Order>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Order>();

            var customer = new Customer
            {
                Name = "John Smith",
                Orders = new[] {
                new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            }
            };

            await conn.InsertOrReplaceWithChildrenAsync(customer, recursive: true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            if (obtainedCustomer == null || obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceAsync: obtainedCustomer or obtainedCustomer.Orders is null");

            if (expectedOrders.Count != obtainedCustomer.Orders.Length)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceAsync: Order count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id || order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Customer mismatch for order {order.Id}");
                if (order.Customer.Orders == null || order.Customer.Orders.Length != expectedOrders.Count)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Customer orders count mismatch for order {order.Id}");
            }

            var newCustomer = new Customer
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[] {
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
            if (obtainedCustomer == null || obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceAsync: obtainedCustomer after replace or obtainedCustomer.Orders is null");

            if (expectedOrders.Count != obtainedCustomer.Orders.Length)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceAsync: Order count mismatch after replace");

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Amount mismatch for order {order.Id} after replace");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Date mismatch for order {order.Id} after replace");
                if (order.Customer == null || order.CustomerId != customer.Id || order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Customer mismatch for order {order.Id} after replace");
                if (order.Customer.Orders == null || order.Customer.Orders.Length != expectedOrders.Count)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Customer orders count mismatch for order {order.Id} after replace");
            }

            return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertOrReplaceAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceAsync: Exception occurred - {ex.Message}");
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
    public static async Task<Tuple<bool, string>> TestOneToManyRecursiveInsertGuidAsync()
    {
        try
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertGuidAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertGuidAsync: obtainedCustomer.Orders is null");
            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertGuidAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuidAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuidAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuidAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuidAsync: Customer mismatch in order {order.Id}");
                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuidAsync: Customer name mismatch in order {order.Id}");
            }

            return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertGuidAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuidAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestOneToManyRecursiveInsertOrReplaceGuidAsync()
    {
        try
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: obtainedCustomer.Orders is null");
            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Customer mismatch in order {order.Id}");
                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Customer name mismatch in order {order.Id}");
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: obtainedCustomer.Orders is null");
            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Customer mismatch in order {order.Id}");
                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Customer name mismatch in order {order.Id}");
            }

            return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertOrReplaceGuidAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuidAsync: Exception occurred - {ex.Message}");
        }
    }
    #endregion

    #region ManyToOneRecursiveInsertAsync
    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsert but inserting
    /// one of the orders instead of the customer
    /// </summary>

    public static async Task<Tuple<bool, string>> TestManyToOneRecursiveInsertAsync()
    {
        try
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertAsync: obtainedCustomer.Orders is null");
            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertAsync: Customer mismatch in order {order.Id}");
                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertAsync: Customer name mismatch in order {order.Id}");
            }

            return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertAsync: Exception occurred - {ex.Message}");
        }
    }

    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplace but inserting
    /// one of the orders instead of the customer
    /// </summary>

    public static async Task<Tuple<bool, string>> TestManyToOneRecursiveInsertOrReplaceAsync()
    {
        try
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceAsync: obtainedCustomer.Orders is null");
            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Customer mismatch in order {order.Id}");
                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Customer name mismatch in order {order.Id}");
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceAsync: obtainedCustomer.Orders is null");
            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Amount mismatch for order {order.Id}");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Date mismatch for order {order.Id}");
                if (order.Customer == null || order.CustomerId != customer.Id)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Customer mismatch in order {order.Id}");
                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Customer name mismatch in order {order.Id}");
            }

            return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertOrReplaceAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceAsync: Exception occurred - {ex.Message}");
        }
    }
    #endregion

    #region ManyToOneRecursiveInsertGuidAsync


    public static async Task<Tuple<bool, string>> TestManyToOneRecursiveInsertGuidAsync()
    {
        try
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuidAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null || obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuidAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuidAsync: Amount mismatch");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuidAsync: Date mismatch");
                if (order.Customer == null || order.CustomerId != customer.Id || order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuidAsync: Customer reference mismatch");
            }

            return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertGuidAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuidAsync: Exception occurred - {ex.Message}");
        }
    }

    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsertGuid but inserting
    /// one of the orders instead of the customer
    /// </summary>

    public static async Task<Tuple<bool, string>> TestManyToOneRecursiveInsertOrReplaceGuidAsync()
    {
        try
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null || obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Amount mismatch");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Date mismatch");
                if (order.Customer == null || order.CustomerId != customer.Id || order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Customer reference mismatch");
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
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: obtainedCustomer is null");
            if (obtainedCustomer.Orders == null || obtainedCustomer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Amount mismatch");
                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Date mismatch");
                if (order.Customer == null || order.CustomerId != customer.Id || order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Customer reference mismatch");
            }

            return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertOrReplaceGuidAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuidAsync: Exception occurred - {ex.Message}");
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

    public static async Task<Tuple<bool, string>> TestManyToManyRecursiveInsertWithSameClassRelationshipAsync()
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

        try
        {
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
            var anthony = new TwitterUser { Name = "Anthony" };
            var peter = new TwitterUser { Name = "Peter" };

            john.FollowingUsers = [peter, thomas];
            thomas.FollowingUsers = [john];
            will.FollowingUsers = [claire];
            claire.FollowingUsers = [will];
            jaime.FollowingUsers = [peter, thomas, mark];
            mark.FollowingUsers = [];
            martha.FollowingUsers = [anthony];
            anthony.FollowingUsers = [peter];
            peter.FollowingUsers = [martha];

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };

            // Only need to insert Jaime and Claire, the other users are contained in these trees
            await conn.InsertAllWithChildrenAsync(new[] { jaime, claire }, recursive: true);

            Action<TwitterUser, TwitterUser> checkUser = (expected, obtained) =>
            {
                if (obtained == null)
                    throw new Exception($"TestManyToManyRecursiveInsertWithSameClassRelationshipAsync: User is null: {expected.Name}");
                if (obtained.Name != expected.Name)
                    throw new Exception($"TestManyToManyRecursiveInsertWithSameClassRelationshipAsync: Name mismatch for user: {expected.Name}");
                if (!obtained.FollowingUsers.OrderBy(u => u.Name).SequenceEqual(expected.FollowingUsers.OrderBy(u => u.Name)))
                    throw new Exception($"TestManyToManyRecursiveInsertWithSameClassRelationshipAsync: Following users mismatch for {expected.Name}");
                var followers = allUsers.Where(u => u.FollowingUsers.Contains(expected));
                if (!obtained.Followers.OrderBy(u => u.Name).SequenceEqual(followers.OrderBy(u => u.Name)))
                    throw new Exception($"TestManyToManyRecursiveInsertWithSameClassRelationshipAsync: Followers mismatch for {expected.Name}");
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

            return new Tuple<bool, string>(true, "TestManyToManyRecursiveInsertWithSameClassRelationshipAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToManyRecursiveInsertWithSameClassRelationshipAsync: Exception occurred - {ex.Message}");
        }
    }

    public static async Task<Tuple<bool, string>> TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync()
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

        try
        {
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
            var anthony = new TwitterUser { Name = "Anthony" };
            var peter = new TwitterUser { Name = "Peter" };

            john.FollowingUsers = [peter, thomas];
            thomas.FollowingUsers = [john];
            will.FollowingUsers = [claire];
            claire.FollowingUsers = [will];
            jaime.FollowingUsers = [peter, thomas, mark];
            mark.FollowingUsers = [];
            martha.FollowingUsers = [anthony];
            anthony.FollowingUsers = [peter];
            peter.FollowingUsers = [martha];

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };

            // Inserts all the objects in the database recursively
            await conn.InsertAllWithChildrenAsync(allUsers, recursive: true);

            // Deletes the entity tree starting at 'Thomas' recursively
            await conn.DeleteAsync(thomas, recursive: true);

            var expectedUsers = new[] { jaime, mark, claire, will };
            var existingUsers = await conn.Table<TwitterUser>().ToListAsync();

            if (!existingUsers.OrderBy(u => u.Name).SequenceEqual(expectedUsers.OrderBy(u => u.Name)))
                return new Tuple<bool, string>(false, "TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync: Users were not deleted correctly");

            return new Tuple<bool, string>(true, "TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync: Exception occurred - {ex.Message}");
        }
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

    public static async Task<Tuple<bool, string>> TestInsertTextBlobPropertiesRecursiveAsync()
    {
        try
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Student>();
            await conn.DropTableAsync<Teacher>();
            await conn.CreateTableAsync<Student>();
            await conn.CreateTableAsync<Teacher>();

            var teacher = new Teacher
            {
                Name = "John Smith",
                Students =
            [
                new Student
                {
                    Name = "Bruce Banner",
                    Address = new Address
                    {
                        Street = "Sesame Street 5",
                        Town = "Gotham City"
                    }
                },
                new Student
                {
                    Name = "Peter Parker",
                    Address = new Address
                    {
                        Street = "Arlington Road 69",
                        Town = "Arkham City"
                    }
                },
                new Student
                {
                    Name = "Steve Rogers",
                    Address = new Address
                    {
                        Street = "28th Street 19",
                        Town = "New York"
                    }
                }
            ]
            };

            await conn.InsertWithChildrenAsync(teacher, recursive: true);

            foreach (var student in teacher.Students)
            {
                var dbStudent = await conn.GetWithChildrenAsync<Student>(student.Id);
                if (dbStudent == null)
                    return new Tuple<bool, string>(false, "TestInsertTextBlobPropertiesRecursiveAsync: dbStudent is null for " + student.Name);
                if (dbStudent.Address == null)
                    return new Tuple<bool, string>(false, "TestInsertTextBlobPropertiesRecursiveAsync: Address is null for " + student.Name);
                if (dbStudent.Address.Street != student.Address.Street)
                    return new Tuple<bool, string>(false, "TestInsertTextBlobPropertiesRecursiveAsync: Street mismatch for " + student.Name);
                if (dbStudent.Address.Town != student.Address.Town)
                    return new Tuple<bool, string>(false, "TestInsertTextBlobPropertiesRecursiveAsync: Town mismatch for " + student.Name);
            }

            return new Tuple<bool, string>(true, "TestInsertTextBlobPropertiesRecursiveAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestInsertTextBlobPropertiesRecursiveAsync: Exception occurred - {ex.Message}");
        }
    }
    #endregion
}
