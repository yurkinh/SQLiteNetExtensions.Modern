using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensions.Attributes;
using SQLite;
 
namespace IntegratedTestsSampleApp.Tests;

public class RecursiveWriteTests
{
    #region OneToOneRecursiveInsert
    [Table("group")] // To test the use of reserved words
    public class Person
    {
        [PrimaryKey, AutoIncrement]
        public int Identifier { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
        public Passport? Passport { get; set; }
    }

    public class Passport
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? PassportNumber { get; set; }

        [ForeignKey(typeof(Person))]
        public int OwnerId { get; set; }

        [OneToOne(ReadOnly = true)]
        public Person? Owner { get; set; }
    }

    public static Tuple<bool, string> TestOneToOneRecursiveInsert()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Passport>();
        conn.DropTable<Person>();
        conn.CreateTable<Passport>();
        conn.CreateTable<Person>();

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
        conn.InsertWithChildren(person, recursive: true);

        var obtainedPerson = conn.Find<Person>(person.Identifier);
        var obtainedPassport = conn.Find<Passport>(person.Passport.Id);

        if (obtainedPerson == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsert failed: Person not found in database.");

        if (obtainedPassport == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsert failed: Passport not found in database.");

        if (obtainedPerson.Name != person.Name)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsert failed: Name mismatch. Expected: {person.Name}, Found: {obtainedPerson.Name}");

        if (obtainedPerson.Surname != person.Surname)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsert failed: Surname mismatch. Expected: {person.Surname}, Found: {obtainedPerson.Surname}");

        if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsert failed: Passport number mismatch. Expected: {person.Passport.PassportNumber}, Found: {obtainedPassport.PassportNumber}");

        if (obtainedPassport.OwnerId != person.Identifier)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsert failed: Passport owner ID mismatch. Expected: {person.Identifier}, Found: {obtainedPassport.OwnerId}");

        return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsert passed.");
    }

    public static Tuple<bool, string> TestOneToOneRecursiveInsertOrReplace()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Passport>();
        conn.DropTable<Person>();
        conn.CreateTable<Passport>();
        conn.CreateTable<Person>();

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
        conn.InsertOrReplaceWithChildren(person, recursive: true);

        var obtainedPerson = conn.Find<Person>(person.Identifier);
        var obtainedPassport = conn.Find<Passport>(person.Passport.Id);

        if (obtainedPerson == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplace failed: Person not found in database.");

        if (obtainedPassport == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplace failed: Passport not found in database.");

        if (obtainedPerson.Name != person.Name)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed: Name mismatch. Expected: {person.Name}, Found: {obtainedPerson.Name}");

        if (obtainedPerson.Surname != person.Surname)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed: Surname mismatch. Expected: {person.Surname}, Found: {obtainedPerson.Surname}");

        if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed: Passport number mismatch. Expected: {person.Passport.PassportNumber}, Found: {obtainedPassport.PassportNumber}");

        if (obtainedPassport.OwnerId != person.Identifier)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed: Passport owner ID mismatch. Expected: {person.Identifier}, Found: {obtainedPassport.OwnerId}");

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
        conn.InsertOrReplaceWithChildren(person, recursive: true);

        obtainedPerson = conn.Find<Person>(person.Identifier);
        obtainedPassport = conn.Find<Passport>(person.Passport.Id);

        if (obtainedPerson == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplace failed: Person not found after replace.");

        if (obtainedPassport == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplace failed: Passport not found after replace.");

        if (obtainedPerson.Name != person.Name)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed after replace: Name mismatch. Expected: {person.Name}, Found: {obtainedPerson.Name}");

        if (obtainedPerson.Surname != person.Surname)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed after replace: Surname mismatch. Expected: {person.Surname}, Found: {obtainedPerson.Surname}");

        if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed after replace: Passport number mismatch. Expected: {person.Passport.PassportNumber}, Found: {obtainedPassport.PassportNumber}");

        if (obtainedPassport.OwnerId != person.Identifier)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplace failed after replace: Passport owner ID mismatch. Expected: {person.Identifier}, Found: {obtainedPassport.OwnerId}");

        return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertOrReplace passed.");
    }

    #endregion

    #region OneToOneRecursiveInsertGuid

    [Table("column")] // To test the use of reserved words
    public class PersonGuid
    {
        [PrimaryKey]
        public Guid Identifier { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
        public PassportGuid? Passport { get; set; }
    }

    public class PassportGuid
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? PassportNumber { get; set; }

        [ForeignKey(typeof(PersonGuid))]
        public Guid OwnerId { get; set; }

        [OneToOne(ReadOnly = true)]
        public PersonGuid? Owner { get; set; }
    }

    public static Tuple<bool, string> TestOneToOneRecursiveInsertGuid()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<PassportGuid>();
        conn.DropTable<PersonGuid>();
        conn.CreateTable<PassportGuid>();
        conn.CreateTable<PersonGuid>();

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
        conn.InsertWithChildren(person, recursive: true);

        var obtainedPerson = conn.Find<PersonGuid>(person.Identifier);
        var obtainedPassport = conn.Find<PassportGuid>(person.Passport.Id);

        if (obtainedPerson == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuid failed: Person not found in database.");

        if (obtainedPassport == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertGuid failed: Passport not found in database.");

        if (obtainedPerson.Name != person.Name)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertGuid failed: Name mismatch. Expected: {person.Name}, Found: {obtainedPerson.Name}");

        if (obtainedPerson.Surname != person.Surname)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertGuid failed: Surname mismatch. Expected: {person.Surname}, Found: {obtainedPerson.Surname}");

        if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertGuid failed: Passport number mismatch. Expected: {person.Passport.PassportNumber}, Found: {obtainedPassport.PassportNumber}");

        if (obtainedPassport.OwnerId != person.Identifier)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertGuid failed: Passport owner ID mismatch. Expected: {person.Identifier}, Found: {obtainedPassport.OwnerId}");

        return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertGuid passed.");
    }

    public static Tuple<bool, string> TestOneToOneRecursiveInsertOrReplaceGuid()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<PassportGuid>();
        conn.DropTable<PersonGuid>();
        conn.CreateTable<PassportGuid>();
        conn.CreateTable<PersonGuid>();

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
        conn.InsertOrReplaceWithChildren(person, recursive: true);

        var obtainedPerson = conn.Find<PersonGuid>(person.Identifier);
        var obtainedPassport = conn.Find<PassportGuid>(person.Passport.Id);

        if (obtainedPerson == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuid failed: Person not found in database.");

        if (obtainedPassport == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuid failed: Passport not found in database.");

        if (obtainedPerson.Name != person.Name)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed: Name mismatch. Expected: {person.Name}, Found: {obtainedPerson.Name}");

        if (obtainedPerson.Surname != person.Surname)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed: Surname mismatch. Expected: {person.Surname}, Found: {obtainedPerson.Surname}");

        if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed: Passport number mismatch. Expected: {person.Passport.PassportNumber}, Found: {obtainedPassport.PassportNumber}");

        if (obtainedPassport.OwnerId != person.Identifier)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed: Passport owner ID mismatch. Expected: {person.Identifier}, Found: {obtainedPassport.OwnerId}");

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
        conn.InsertOrReplaceWithChildren(person, recursive: true);

        obtainedPerson = conn.Find<PersonGuid>(person.Identifier);
        obtainedPassport = conn.Find<PassportGuid>(person.Passport.Id);

        if (obtainedPerson == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuid failed: Person not found after replace.");

        if (obtainedPassport == null)
            return new Tuple<bool, string>(false, "TestOneToOneRecursiveInsertOrReplaceGuid failed: Passport not found after replace.");

        if (obtainedPerson.Name != person.Name)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed after replace: Name mismatch. Expected: {person.Name}, Found: {obtainedPerson.Name}");

        if (obtainedPerson.Surname != person.Surname)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed after replace: Surname mismatch. Expected: {person.Surname}, Found: {obtainedPerson.Surname}");

        if (obtainedPassport.PassportNumber != person.Passport.PassportNumber)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed after replace: Passport number mismatch. Expected: {person.Passport.PassportNumber}, Found: {obtainedPassport.PassportNumber}");

        if (obtainedPassport.OwnerId != person.Identifier)
            return new Tuple<bool, string>(false, $"TestOneToOneRecursiveInsertOrReplaceGuid failed after replace: Passport owner ID mismatch. Expected: {person.Identifier}, Found: {obtainedPassport.OwnerId}");

        return new Tuple<bool, string>(true, "TestOneToOneRecursiveInsertOrReplaceGuid passed.");
    }
    #endregion

    #region OneToManyRecursiveInsert
    [Table("select")] // To test the use of reserved words
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
        public Order[]? Orders { get; set; }
    }

    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public float Amount { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey(typeof(Customer))]
        public int CustomerId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
        public Customer? Customer { get; set; }
    }

    public static Tuple<bool, string> TestOneToManyRecursiveInsert()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Customer>();
        conn.DropTable<Order>();
        conn.CreateTable<Customer>();
        conn.CreateTable<Order>();

        var customer = new Customer
        {
            Name = "John Smith",
            Orders =
            [
                new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            ]
        };

        conn.InsertWithChildren(customer, recursive: true);

        // Warning suppression: In this test logic, Orders is explicitly set above.
        var expectedOrders = customer.Orders!.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsert failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsert failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsert failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsert passed.");
    }

    public static Tuple<bool, string> TestOneToManyRecursiveInsertOrReplace()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Customer>();
        conn.DropTable<Order>();
        conn.CreateTable<Customer>();
        conn.CreateTable<Order>();

        var customer = new Customer
        {
            Name = "John Smith",
            Orders =
            [
                new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            ]
        };

        conn.InsertOrReplaceWithChildren(customer);

        var expectedOrders = customer.Orders!.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplace failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplace failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        var newCustomer = new Customer
        {
            Id = customer.Id,
            Name = "John Smith",
            Orders =
            [
                new Order { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                new Order { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                new Order { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                new Order { Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                new Order { Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
            ]
        };

        customer = newCustomer;

        conn.InsertOrReplaceWithChildren(customer, recursive: true);

        expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplace failed after replace: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplace failed after replace: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplace failed after replace: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertOrReplace passed.");
    }

    #endregion

    #region OneToManyRecursiveInsertGuid
    public class CustomerGuid
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
        public OrderGuid[]? Orders { get; set; }
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
        public CustomerGuid? Customer { get; set; }
    }

    public static Tuple<bool, string> TestOneToManyRecursiveInsertGuid()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<CustomerGuid>();
        conn.DropTable<OrderGuid>();
        conn.CreateTable<CustomerGuid>();
        conn.CreateTable<OrderGuid>();

        var customer = new CustomerGuid
        {
            Id = Guid.NewGuid(),
            Name = "John Smith",
            Orders =
            [
                new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            ]
        };

        conn.InsertWithChildren(customer, recursive: true);

        var expectedOrders = customer.Orders!.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<CustomerGuid>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertGuid failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertGuid failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertGuid failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertGuid passed.");
    }

    public static Tuple<bool, string> TestOneToManyRecursiveInsertOrReplaceGuid()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<CustomerGuid>();
        conn.DropTable<OrderGuid>();
        conn.CreateTable<CustomerGuid>();
        conn.CreateTable<OrderGuid>();

        var customer = new CustomerGuid
        {
            Id = Guid.NewGuid(),
            Name = "John Smith",
            Orders =
                [
                new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            ]
        };

        conn.InsertOrReplaceWithChildren(customer, recursive: true);

        var expectedOrders = customer.Orders!.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<CustomerGuid>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuid failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        var newCustomer = new CustomerGuid
        {
            Id = customer.Id,
            Name = "John Smith",
            Orders =
            [
            new OrderGuid { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
            new OrderGuid { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
            new OrderGuid { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
        ]
        };

        customer = newCustomer;

        conn.InsertOrReplaceWithChildren(customer, recursive: true);

        expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        obtainedCustomer = conn.GetWithChildren<CustomerGuid>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer not found after replace.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestOneToManyRecursiveInsertOrReplaceGuid failed: Orders not found after replace.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Order count mismatch after replace. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Amount mismatch after replace for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Date mismatch after replace for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestOneToManyRecursiveInsertOrReplaceGuid failed: Customer not found after replace for Order {order.Id}");
        }

        return new Tuple<bool, string>(true, "TestOneToManyRecursiveInsertOrReplaceGuid passed.");
    }
    #endregion

    #region ManyToOneRecursiveInsert
    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsert but inserting
    /// one of the orders instead of the customer
    /// </summary>
    public static Tuple<bool, string> TestManyToOneRecursiveInsert()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Customer>();
        conn.DropTable<Order>();
        conn.CreateTable<Customer>();
        conn.CreateTable<Order>();

        var customer = new Customer
        {
            Name = "John Smith",
            Orders =
            [
            new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
            new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
            new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
            new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
            new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
        ]
        };

        // Insert one of the orders instead of the customer
        customer.Orders[0].Customer = customer;
        conn.InsertWithChildren(customer.Orders[0], recursive: true);

        var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsert failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsert failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsert failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsert passed.");
    }
    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplace but inserting
    /// one of the orders instead of the customer
    /// </summary>
    public static Tuple<bool, string> TestManyToOneRecursiveInsertOrReplace()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Customer>();
        conn.DropTable<Order>();
        conn.CreateTable<Customer>();
        conn.CreateTable<Order>();

        var customer = new Customer
        {
            Name = "John Smith",
            Orders =
            [
                new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
            ]
        };

        // Insert any of the orders instead of the customer
        customer.Orders[0].Customer = customer;
        conn.InsertOrReplaceWithChildren(customer.Orders[0], recursive: true);

        var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplace failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplace failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        var newCustomer = new Customer
        {
            Id = customer.Id,
            Name = "John Smith",
            Orders =
            [
            new Order { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
            new Order { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
            new Order { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
            new Order { Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
            new Order { Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
        ]
        };

        customer = newCustomer;

        // Insert any of the orders instead of the customer
        customer.Orders[0].Customer = customer; // Required to complete the entity tree
        conn.InsertOrReplaceWithChildren(customer.Orders[0], recursive: true);

        expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplace failed: Customer not found after replacement.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplace failed: Orders not found after replacement.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Order count mismatch after replacement. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Amount mismatch after replacement for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Date mismatch after replacement for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer not found for Order {order.Id} after replacement.");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: CustomerId mismatch for Order {order.Id} after replacement. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Id mismatch for Order {order.Id} after replacement. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Name mismatch for Order {order.Id} after replacement. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Orders not found for Order {order.Id} after replacement.");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplace failed: Customer Orders count mismatch for Order {order.Id} after replacement. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertOrReplace passed.");
    }
    #endregion

    #region ManyToOneRecursiveInsertGuid
    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsertGuid but inserting
    /// one of the orders instead of the customer
    /// </summary>
    public static Tuple<bool, string> TestManyToOneRecursiveInsertGuid()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<CustomerGuid>();
        conn.DropTable<OrderGuid>();
        conn.CreateTable<CustomerGuid>();
        conn.CreateTable<OrderGuid>();

        var customer = new CustomerGuid
        {
            Id = Guid.NewGuid(),
            Name = "John Smith",
            Orders =
            [
            new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
        ]
        };

        // Insert any of the orders instead of the customer
        customer.Orders[0].Customer = customer; // Required to complete the entity tree
        conn.InsertWithChildren(customer.Orders[0], recursive: true);

        var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<CustomerGuid>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuid failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertGuid failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertGuid failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertGuid passed.");
    }
    /// <summary>
    /// This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplaceGuid but inserting
    /// one of the orders instead of the customer
    /// </summary>
    public static Tuple<bool, string> TestManyToOneRecursiveInsertOrReplaceGuid()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<CustomerGuid>();
        conn.DropTable<OrderGuid>();
        conn.CreateTable<CustomerGuid>();
        conn.CreateTable<OrderGuid>();

        var customer = new CustomerGuid
        {
            Id = Guid.NewGuid(),
            Name = "John Smith",
            Orders =
            [
            new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
            new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
        ]
        };

        // Insert any of the orders instead of the customer
        customer.Orders[0].Customer = customer; // Required to complete the entity tree
        conn.InsertOrReplaceWithChildren(customer.Orders[0], recursive: true);

        var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        var obtainedCustomer = conn.GetWithChildren<CustomerGuid>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer not found in database.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuid failed: Orders not found for customer.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Order count mismatch. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Amount mismatch for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Date mismatch for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer not found for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: CustomerId mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Id mismatch for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Name mismatch for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Orders not found for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Orders count mismatch for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        // Now modify the customer and insert again
        var newCustomer = new CustomerGuid
        {
            Id = customer.Id,
            Name = "John Smith",
            Orders =
            [
                new OrderGuid { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                new OrderGuid { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                new OrderGuid { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                new OrderGuid { Id = Guid.NewGuid(), Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
            ]
        };

        customer = newCustomer;

        // Insert any of the orders instead of the customer
        customer.Orders[0].Customer = customer; // Required to complete the entity tree
        conn.InsertOrReplaceWithChildren(customer.Orders[0], recursive: true);

        expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

        obtainedCustomer = conn.GetWithChildren<CustomerGuid>(customer.Id, recursive: true);
        if (obtainedCustomer == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer not found after replace.");

        if (obtainedCustomer.Orders == null)
            return new Tuple<bool, string>(false, "TestManyToOneRecursiveInsertOrReplaceGuid failed: Orders not found after replace.");

        if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Order count mismatch after replace. Expected: {expectedOrders.Count}, Found: {obtainedCustomer.Orders.Length}");

        foreach (var order in obtainedCustomer.Orders)
        {
            var expectedOrder = expectedOrders[order.Id];

            if (expectedOrder.Amount != order.Amount)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Amount mismatch after replace for Order {order.Id}. Expected: {expectedOrder.Amount}, Found: {order.Amount}");

            if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Date mismatch after replace for Order {order.Id}. Expected: {expectedOrder.Date}, Found: {order.Date}");

            if (order.Customer == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer not found after replace for Order {order.Id}");

            if (order.CustomerId != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: CustomerId mismatch after replace for Order {order.Id}. Expected: {customer.Id}, Found: {order.CustomerId}");

            if (order.Customer.Id != customer.Id)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Id mismatch after replace for Order {order.Id}. Expected: {customer.Id}, Found: {order.Customer.Id}");

            if (order.Customer.Name != customer.Name)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Name mismatch after replace for Order {order.Id}. Expected: {customer.Name}, Found: {order.Customer.Name}");

            if (order.Customer.Orders == null)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Orders not found after replace for Order {order.Id}");

            if (order.Customer.Orders.Length != expectedOrders.Count)
                return new Tuple<bool, string>(false, $"TestManyToOneRecursiveInsertOrReplaceGuid failed: Customer Orders count mismatch after replace for Order {order.Id}. Expected: {expectedOrders.Count}, Found: {order.Customer.Orders.Length}");
        }

        return new Tuple<bool, string>(true, "TestManyToOneRecursiveInsertOrReplaceGuid passed.");
    }
    #endregion

    #region ManyToManyCascadeWithSameClassRelationship
    public class TwitterUser
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        [ManyToMany(typeof(FollowerLeaderRelationshipTable), "LeaderId", "Followers",
            CascadeOperations = CascadeOperation.All)]
        public List<TwitterUser>? FollowingUsers { get; set; }

        // ReadOnly is required because we're not specifying the followers manually, but want to obtain them from database
        [ManyToMany(typeof(FollowerLeaderRelationshipTable), "FollowerId", "FollowingUsers",
            CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true)]
        public List<TwitterUser>? Followers { get; set; }

        public override bool Equals(object? obj)
        {
            var other = obj as TwitterUser;
            return other != null && (Name?.Equals(other.Name) ?? other.Name == null);
        }
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
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

    public static Tuple<bool, string> TestManyToManyRecursiveInsertWithSameClassRelationship()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<TwitterUser>();
        conn.DropTable<FollowerLeaderRelationshipTable>();
        conn.CreateTable<TwitterUser>();
        conn.CreateTable<FollowerLeaderRelationshipTable>();

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

        conn.InsertAllWithChildren(new[] { jaime, claire }, recursive: true);

        // Updated signature to accept nullable 'obtained' since FirstOrDefault returns null
        Tuple<bool, string> CheckUser(TwitterUser expected, TwitterUser? obtained)
        {
            if (obtained == null)
                return new Tuple<bool, string>(false, $"User is null: {expected.Name}");

            if (expected.Name != obtained.Name)
                return new Tuple<bool, string>(false, $"Expected name '{expected.Name}', but got '{obtained.Name}'");

            // We can safely assume Followers and FollowingUsers are not null here because they are lists
            // initialized by the ORM, but a null check is safer in production code. 
            // For this test, we assume standard behavior.
            var followers = allUsers.Where(u => u.FollowingUsers != null && u.FollowingUsers.Contains(expected));

            // Null-checks added for safety
            var expectedFollowing = expected.FollowingUsers ?? [];
            var obtainedFollowing = obtained.FollowingUsers ?? [];
            var obtainedFollowers = obtained.Followers ?? [];

            var followingCheck = expectedFollowing.OrderBy(u => u.Name).SequenceEqual(obtainedFollowing.OrderBy(u => u.Name));
            if (!followingCheck)
                return new Tuple<bool, string>(false, $"Following users for '{expected.Name}' do not match.");

            var followersCheck = followers.OrderBy(u => u.Name).SequenceEqual(obtainedFollowers.OrderBy(u => u.Name));
            if (!followersCheck)
                return new Tuple<bool, string>(false, $"Followers for '{expected.Name}' do not match.");

            return new Tuple<bool, string>(true, $"User '{expected.Name}' passed all checks.");
        }

        var obtainedThomas = conn.GetWithChildren<TwitterUser>(thomas.Id, recursive: true);
        var result = CheckUser(thomas, obtainedThomas);
        if (!result.Item1) return result;

        var obtainedJohn = obtainedThomas!.FollowingUsers!.FirstOrDefault(u => u.Id == john.Id);
        result = CheckUser(john, obtainedJohn);
        if (!result.Item1) return result;

        var obtainedPeter = obtainedJohn!.FollowingUsers!.FirstOrDefault(u => u.Id == peter.Id);
        result = CheckUser(peter, obtainedPeter);
        if (!result.Item1) return result;

        var obtainedMartha = obtainedPeter!.FollowingUsers!.FirstOrDefault(u => u.Id == martha.Id);
        result = CheckUser(martha, obtainedMartha);
        if (!result.Item1) return result;

        var obtainedAnthony = obtainedMartha!.FollowingUsers!.FirstOrDefault(u => u.Id == anthony.Id);
        result = CheckUser(anthony, obtainedAnthony);
        if (!result.Item1) return result;

        var obtainedJaime = obtainedThomas.Followers!.FirstOrDefault(u => u.Id == jaime.Id);
        result = CheckUser(jaime, obtainedJaime);
        if (!result.Item1) return result;

        var obtainedMark = obtainedJaime!.FollowingUsers!.FirstOrDefault(u => u.Id == mark.Id);
        result = CheckUser(mark, obtainedMark);
        if (!result.Item1) return result;

        return new Tuple<bool, string>(true, "All checks passed successfully.");
    }

    public static Tuple<bool, string> TestManyToManyRecursiveDeleteWithSameClassRelationship()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<TwitterUser>();
        conn.DropTable<FollowerLeaderRelationshipTable>();
        conn.CreateTable<TwitterUser>();
        conn.CreateTable<FollowerLeaderRelationshipTable>();

        var john = new TwitterUser { Name = "John" };
        var thomas = new TwitterUser { Name = "Thomas" };
        var will = new TwitterUser { Name = "Will" };
        var claire = new TwitterUser { Name = "Claire" };
        var jaime = new TwitterUser { Name = "Jaime" };
        var mark = new TwitterUser { Name = "Mark" };
        var martha = new TwitterUser { Name = "Martha" };
        var anthony = new TwitterUser { Name = "anthony" };
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
        conn.InsertAllWithChildren(allUsers, recursive: true);

        // Deletes the entity tree starting at 'Thomas' recursively
        conn.Delete(thomas, recursive: true);

        var expectedUsers = new[] { jaime, mark, claire, will };
        var existingUsers = conn.Table<TwitterUser>().ToList();

        // Check that the users have been deleted and only the users outside the 'Thomas' tree still exist
        if (!existingUsers.OrderBy(u => u.Name).SequenceEqual(expectedUsers.OrderBy(u => u.Name)))
        {
            return Tuple.Create(false, $"{nameof(TestManyToManyRecursiveDeleteWithSameClassRelationship)}: Users were not deleted correctly.");
        }

        return Tuple.Create(true, $"{nameof(TestManyToManyRecursiveDeleteWithSameClassRelationship)}: Test passed.");
    }

    #endregion

    #region InsertTextBlobPropertiesRecursive
    class Teacher
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
        public List<Student>? Students { get; set; }
    }

    class Student
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Name { get; set; }

        [ManyToOne]
        public Teacher? Teacher { get; set; }

        [TextBlob("AddressBlob")]
        public Address? Address { get; set; }

        [ForeignKey(typeof(Teacher))]
        public int TeacherId { get; set; }
        public String? AddressBlob { get; set; }

    }

    class Address
    {
        public string? Street { get; set; }
        public string? Town { get; set; }
    }

    public static Tuple<bool, string> TestInsertTextBlobPropertiesRecursive()
    {
        var conn = Utils.CreateConnection();
        conn.DropTable<Student>();
        conn.DropTable<Teacher>();
        conn.CreateTable<Student>();
        conn.CreateTable<Teacher>();

        var teacher = new Teacher
        {
            Name = "John Smith",
            Students = [
            new() {
                Name = "Bruce Banner",
                Address = new Address {
                    Street = "Sesame Street 5",
                    Town = "Gotham City"
                }
            },
            new() {
                Name = "Peter Parker",
                Address = new Address {
                    Street = "Arlington Road 69",
                    Town = "Arkham City"
                }
            },
            new() {
                Name = "Steve Rogers",
                Address = new Address {
                    Street = "28th Street 19",
                    Town = "New York"
                }
            }
        ]
        };

        conn.InsertWithChildren(teacher, recursive: true);

        // Suppressing null warning as we know Students is initialized above
        foreach (var student in teacher.Students!)
        {
            var dbStudent = conn.GetWithChildren<Student>(student.Id);

            if (dbStudent == null)
            {
                return Tuple.Create(false, $"{nameof(TestInsertTextBlobPropertiesRecursive)}: Student '{student.Name}' not found in the database.");
            }

            if (dbStudent.Address == null)
            {
                return Tuple.Create(false, $"{nameof(TestInsertTextBlobPropertiesRecursive)}: Address for student '{student.Name}' is null.");
            }

            if (dbStudent.Address.Street != student.Address!.Street)
            {
                return Tuple.Create(false, $"{nameof(TestInsertTextBlobPropertiesRecursive)}: Street mismatch for student '{student.Name}'. Expected: '{student.Address.Street}', Found: '{dbStudent.Address.Street}'.");
            }

            if (dbStudent.Address.Town != student.Address.Town)
            {
                return Tuple.Create(false, $"{nameof(TestInsertTextBlobPropertiesRecursive)}: Town mismatch for student '{student.Name}'. Expected: '{student.Address.Town}', Found: '{dbStudent.Address.Town}'.");
            }
        }

        return Tuple.Create(true, $"{nameof(TestInsertTextBlobPropertiesRecursive)}: Test passed.");
    }

    #endregion
}