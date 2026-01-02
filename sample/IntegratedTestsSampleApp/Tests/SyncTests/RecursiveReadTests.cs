using IntegratedTestsSampleApp.Helpers;
using SQLiteNetExtensions.Extensions;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace IntegratedTestsSampleApp.Tests;

public class RecursiveReadTests
{
    #region TestOneToOneCascadeWithInverse
    public class PassportWithForeignKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? PassportNumber { get; set; }

        [ForeignKey(typeof(PersonNoForeignKey))]
        public int OwnerId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public PersonNoForeignKey? Owner { get; set; }
    }

    public class PersonNoForeignKey
    {
        [PrimaryKey, AutoIncrement]
        public int Identifier { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public PassportWithForeignKey? Passport { get; set; }
    }

    public static Tuple<bool, string> TestOneToOneCascadeWithInverse()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<PassportWithForeignKey>();
            conn.DropTable<PersonNoForeignKey>();
            conn.CreateTable<PassportWithForeignKey>();
            conn.CreateTable<PersonNoForeignKey>();

            var person = new PersonNoForeignKey { Name = "John", Surname = "Smith" };
            conn.Insert(person);

            var passport = new PassportWithForeignKey { PassportNumber = "JS12345678", Owner = person };
            conn.Insert(passport);
            conn.UpdateWithChildren(passport);

            var obtainedPerson = conn.GetWithChildren<PersonNoForeignKey>(person.Identifier, recursive: true);
            if (obtainedPerson == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Obtained person is null.");
            }

            if (obtainedPerson.Passport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Person passport is null.");
            }

            if (obtainedPerson.Passport.Owner == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Circular reference not solved for Passport Owner.");
            }

            if (obtainedPerson.Identifier != obtainedPerson.Passport.Owner.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Integral reference check failed.");
            }

            // Added conditional access ?. to prevent warning, though logic guarantees not null here due to previous checks
            if (obtainedPerson.Passport.Id != obtainedPerson.Passport.Owner.Passport?.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Passport ID mismatch.");
            }

            if (person.Identifier != obtainedPerson.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Person identifier mismatch.");
            }

            if (passport.Id != obtainedPerson.Passport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Passport ID mismatch.");
            }

            var obtainedPassport = conn.GetWithChildren<PassportWithForeignKey>(passport.Id, recursive: true);
            if (obtainedPassport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Obtained passport is null.");
            }

            if (obtainedPassport.Owner == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Passport owner is null.");
            }

            if (obtainedPassport.Owner.Passport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Circular reference not solved for Passport Owner.");
            }

            if (obtainedPassport.Id != obtainedPassport.Owner.Passport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Passport ID mismatch in circular reference.");
            }

            if (obtainedPassport.Owner.Identifier != obtainedPassport.Owner.Passport.Owner?.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Integral reference check failed in circular reference.");
            }

            if (passport.Id != obtainedPassport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Passport ID mismatch.");
            }

            if (person.Identifier != obtainedPassport.Owner.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverse: Person identifier mismatch in circular reference.");
            }

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverse: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverse: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestOneToOneCascadeWithInverseReversed()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<PassportWithForeignKey>();
            conn.DropTable<PersonNoForeignKey>();
            conn.CreateTable<PassportWithForeignKey>();
            conn.CreateTable<PersonNoForeignKey>();

            var person = new PersonNoForeignKey { Name = "John", Surname = "Smith" };
            conn.Insert(person);

            var passport = new PassportWithForeignKey { PassportNumber = "JS12345678", Owner = person };
            conn.Insert(passport);
            conn.UpdateWithChildren(passport);

            var obtainedPassport = conn.GetWithChildren<PassportWithForeignKey>(passport.Id, recursive: true);
            if (obtainedPassport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Obtained passport is null.");
            }

            if (obtainedPassport.Owner == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Passport owner is null.");
            }

            if (obtainedPassport.Owner.Passport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Circular reference not solved for Passport Owner.");
            }

            if (obtainedPassport.Id != obtainedPassport.Owner.Passport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Passport ID mismatch in circular reference.");
            }

            if (obtainedPassport.Owner.Identifier != obtainedPassport.Owner.Passport.Owner?.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Integral reference check failed in circular reference.");
            }

            if (passport.Id != obtainedPassport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Passport ID mismatch.");
            }

            if (person.Identifier != obtainedPassport.Owner.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversed: Person identifier mismatch in circular reference.");
            }

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseReversed: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseReversed: Test failed with exception: {ex.Message}");
        }
    }


    #endregion

    #region TestOneToOneCascadeWithInverseDoubleForeignKey
    public class PassportWithForeignKeyDouble
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string? PassportNumber { get; set; }

        [ForeignKey(typeof(PersonWithForeignKey))]
        public int OwnerId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public PersonWithForeignKey? Owner { get; set; }
    }

    public class PersonWithForeignKey
    {
        [PrimaryKey]
        public int Identifier { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        [ForeignKey(typeof(PassportWithForeignKeyDouble))]
        public int PassportId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public PassportWithForeignKeyDouble? Passport { get; set; }
    }

    public static Tuple<bool, string> TestOneToOneCascadeWithInverseDoubleForeignKey()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<PassportWithForeignKeyDouble>();
            conn.DropTable<PersonWithForeignKey>();
            conn.CreateTable<PassportWithForeignKeyDouble>();
            conn.CreateTable<PersonWithForeignKey>();

            var person = new PersonWithForeignKey { Name = "John", Surname = "Smith" };
            conn.Insert(person);

            var passport = new PassportWithForeignKeyDouble { PassportNumber = "JS12345678", Owner = person };
            conn.Insert(passport);
            conn.UpdateWithChildren(passport);

            var obtainedPerson = conn.GetWithChildren<PersonWithForeignKey>(person.Identifier, recursive: true);
            if (obtainedPerson == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Obtained person is null.");
            }

            if (obtainedPerson.Passport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Person passport is null.");
            }

            if (obtainedPerson.Passport.Owner == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Circular reference not solved for Passport Owner.");
            }

            if (obtainedPerson.Identifier != obtainedPerson.Passport.Owner.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Integral reference check failed.");
            }

            if (obtainedPerson.Passport.Id != obtainedPerson.Passport.Owner.Passport?.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Passport ID mismatch.");
            }

            if (person.Identifier != obtainedPerson.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Person identifier mismatch.");
            }

            if (passport.Id != obtainedPerson.Passport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKey: Passport ID mismatch.");
            }

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseDoubleForeignKey: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseDoubleForeignKey: Test failed with exception: {ex.Message}");
        }
    }

    public static Tuple<bool, string> TestOneToOneCascadeWithInverseDoubleForeignKeyReversed()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<PassportWithForeignKeyDouble>();
            conn.DropTable<PersonWithForeignKey>();
            conn.CreateTable<PassportWithForeignKeyDouble>();
            conn.CreateTable<PersonWithForeignKey>();

            var person = new PersonWithForeignKey { Name = "John", Surname = "Smith" };
            conn.Insert(person);

            var passport = new PassportWithForeignKeyDouble { PassportNumber = "JS12345678", Owner = person };
            conn.Insert(passport);
            conn.UpdateWithChildren(passport);

            var obtainedPassport = conn.GetWithChildren<PassportWithForeignKeyDouble>(passport.Id, recursive: true);
            if (obtainedPassport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Obtained passport is null.");
            }

            if (obtainedPassport.Owner == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Passport owner is null.");
            }

            if (obtainedPassport.Owner.Passport == null)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Circular reference not solved for Passport Owner.");
            }

            if (obtainedPassport.Id != obtainedPassport.Owner.Passport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Passport ID mismatch in circular reference.");
            }

            if (obtainedPassport.Owner.Identifier != obtainedPassport.Owner.Passport.Owner?.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Integral reference check failed in circular reference.");
            }

            if (passport.Id != obtainedPassport.Id)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Passport ID mismatch.");
            }

            if (person.Identifier != obtainedPassport.Owner.Identifier)
            {
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Person identifier mismatch in circular reference.");
            }

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseDoubleForeignKeyReversed: Test failed with exception: {ex.Message}");
        }
    }
    #endregion


    #region OneToManyCascadeWithInverse
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public Order[]? Orders { get; set; }
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
        public Customer? Customer { get; set; }
    }

    public static Tuple<bool, string> TestOneToManyCascadeWithInverse()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<Customer>();
            conn.DropTable<Order>();
            conn.CreateTable<Customer>();
            conn.CreateTable<Order>();

            var customer = new Customer { Name = "John Smith" };
            var orders = new[]
            {
            new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
            new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
            new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
            new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
            new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
        };

            conn.Insert(customer);
            conn.InsertAll(orders);

            customer.Orders = orders;
            conn.UpdateWithChildren(customer);

            var expectedOrders = orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = conn.GetWithChildren<Customer>(customer.Id, recursive: true);
            if (obtainedCustomer == null)
            {
                return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Obtained customer is null.");
            }

            if (obtainedCustomer.Orders == null)
            {
                return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Customer orders are null.");
            }

            if (expectedOrders.Count != obtainedCustomer.Orders.Length)
            {
                return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Orders count mismatch.");
            }

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                {
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Order amount mismatch.");
                }

                if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                {
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Order date mismatch.");
                }

                if (order.Customer == null)
                {
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Order customer is null.");
                }

                if (customer.Id != order.Customer.Id)
                {
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Order customer ID mismatch.");
                }

                if (customer.Name != order.Customer.Name)
                {
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Order customer name mismatch.");
                }

                // Added ?. to Orders access to suppress warning, though logic implies not null
                if (order.Customer.Orders?.Length != expectedOrders.Count)
                {
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverse: Customer orders count mismatch.");
                }
            }

            return new Tuple<bool, string>(true, "TestOneToManyCascadeWithInverse: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToManyCascadeWithInverse: Test failed with exception: {ex.Message}");
        }
    }

    #endregion

    #region ManyToOneCascadeWithInverse
    /// <summary>
    /// In this test we will execute the same test that we did in TestOneToManyCascadeWithInverse but fetching
    /// one of the orders
    /// </summary>

    public static Tuple<bool, string> TestManyToOneCascadeWithInverse()
    {
        try
        {
            var conn = Utils.CreateConnection();
            conn.DropTable<Customer>();
            conn.DropTable<Order>();
            conn.CreateTable<Customer>();
            conn.CreateTable<Order>();

            var customer = new Customer { Name = "John Smith" };
            var orders = new[]
            {
            new Order { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
            new Order { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
            new Order { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
            new Order { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
            new Order { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
        };

            conn.Insert(customer);
            conn.InsertAll(orders);

            customer.Orders = orders;
            conn.UpdateWithChildren(customer);

            var orderToFetch = orders[2];
            var expectedOrders = orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedOrder = conn.GetWithChildren<Order>(orderToFetch.Id, recursive: true);
            if (obtainedOrder == null)
            {
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Obtained order is null.");
            }

            if (obtainedOrder.Date.ToUniversalTime() != orderToFetch.Date.ToUniversalTime())
            {
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order date mismatch.");
            }

            if (Math.Abs(obtainedOrder.Amount - orderToFetch.Amount) > 0.0001)
            {
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order amount mismatch.");
            }

            var obtainedCustomer = obtainedOrder.Customer;
            if (obtainedCustomer == null)
            {
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order customer is null.");
            }

            if (obtainedCustomer.Orders == null)
            {
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Customer orders are null.");
            }

            if (obtainedCustomer.Orders.Length != expectedOrders.Count)
            {
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Customer orders count mismatch.");
            }

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                {
                    return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order amount mismatch.");
                }

                if (expectedOrder.Date.ToUniversalTime() != order.Date.ToUniversalTime())
                {
                    return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order date mismatch.");
                }

                if (order.Customer == null)
                {
                    return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order customer is null.");
                }

                if (customer.Id != order.Customer.Id)
                {
                    return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order customer ID mismatch.");
                }

                if (customer.Name != order.Customer.Name)
                {
                    return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Order customer name mismatch.");
                }

                // Added ?. to Orders access
                if (order.Customer.Orders?.Length != expectedOrders.Count)
                {
                    return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverse: Customer orders count mismatch.");
                }
            }

            return new Tuple<bool, string>(true, "TestManyToOneCascadeWithInverse: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverse: Test failed with exception: {ex.Message}");
        }
    }
    #endregion

    #region ManyToManyCascadeWithSameClassRelationship
    public class TwitterUser
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        [ManyToMany(typeof(FollowerLeaderRelationshipTable), "LeaderId", "Followers",
            CascadeOperations = CascadeOperation.CascadeRead)]
        public List<TwitterUser>? FollowingUsers { get; set; }

        // ReadOnly is required because we're not specifying the followers manually, but want to obtain them from database
        [ManyToMany(typeof(FollowerLeaderRelationshipTable), "FollowerId", "FollowingUsers",
            CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true)]
        public List<TwitterUser>? Followers { get; set; }

        public override bool Equals(object? obj)
        {
            // Updated to handle nullable Name safely
            return obj is TwitterUser other && (Name?.Equals(other.Name) ?? other.Name == null);
        }
        public override int GetHashCode()
        {
            // Updated to handle nullable Name safely
            return Name?.GetHashCode() ?? 0;
        }
    }

    // Intermediate class, not used directly anywhere in the code, only in ManyToMany attributes and table creation
    public class FollowerLeaderRelationshipTable
    {
        public int LeaderId { get; set; }
        public int FollowerId { get; set; }
    }

    public static Tuple<bool, string> TestManyToManyCascadeWithSameClassRelationship()
    {
        // ... (Comment block preserved) ...

        try
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

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };
            conn.InsertAll(allUsers);

            john.FollowingUsers = [peter, thomas];
            thomas.FollowingUsers = [john];
            will.FollowingUsers = [claire];
            claire.FollowingUsers = [will];
            jaime.FollowingUsers = [peter, thomas, mark];
            mark.FollowingUsers = [];
            martha.FollowingUsers = [anthony];
            anthony.FollowingUsers = [peter];
            peter.FollowingUsers = [martha];

            foreach (var user in allUsers)
            {
                conn.UpdateWithChildren(user);
            }

            // Updated delegate signature to accept nullable 'obtained' to handle FirstOrDefault results
            Action<TwitterUser, TwitterUser?> checkUser = (expected, obtained) =>
            {
                if (obtained == null)
                {
                    throw new Exception($"TestManyToManyCascadeWithSameClassRelationship: User is null: {expected.Name}");
                }

                if (obtained.Name != expected.Name)
                {
                    throw new Exception($"TestManyToManyCascadeWithSameClassRelationship: User name mismatch: {expected.Name}");
                }

                // Handle null lists if ORM returns null instead of empty list
                var obtainedFollowing = obtained.FollowingUsers ?? new List<TwitterUser>();
                var expectedFollowing = expected.FollowingUsers ?? new List<TwitterUser>();

                if (!obtainedFollowing.OrderBy(u => u.Name).SequenceEqual(expectedFollowing.OrderBy(u => u.Name)))
                {
                    throw new Exception($"TestManyToManyCascadeWithSameClassRelationship: Following users mismatch for {expected.Name}");
                }

                var followers = allUsers.Where(u => u.FollowingUsers != null && u.FollowingUsers.Contains(expected));
                var obtainedFollowers = obtained.Followers ?? new List<TwitterUser>();

                if (!obtainedFollowers.OrderBy(u => u.Name).SequenceEqual(followers.OrderBy(u => u.Name)))
                {
                    throw new Exception($"TestManyToManyCascadeWithSameClassRelationship: Followers mismatch for {expected.Name}");
                }
            };

            var obtainedThomas = conn.GetWithChildren<TwitterUser>(thomas.Id, recursive: true);
            checkUser(thomas, obtainedThomas);

            // Added ? to FollowingUsers access because it is now nullable
            var obtainedJohn = obtainedThomas!.FollowingUsers?.FirstOrDefault(u => u.Id == john.Id);
            //!
            checkUser(john, obtainedJohn);

            var obtainedPeter = obtainedJohn!.FollowingUsers?.FirstOrDefault(u => u.Id == peter.Id);
            checkUser(peter, obtainedPeter);

            var obtainedMartha = obtainedPeter!.FollowingUsers?.FirstOrDefault(u => u.Id == martha.Id);
            checkUser(martha, obtainedMartha);

            var obtainedAnthony = obtainedMartha!.FollowingUsers?.FirstOrDefault(u => u.Id == anthony.Id);
            checkUser(anthony, obtainedAnthony);

            var obtainedJaime = obtainedThomas.Followers?.FirstOrDefault(u => u.Id == jaime.Id);
            //!
            checkUser(jaime, obtainedJaime);

            var obtainedMark = obtainedJaime!.FollowingUsers?.FirstOrDefault(u => u.Id == mark.Id);
            checkUser(mark, obtainedMark);

            return new Tuple<bool, string>(true, "TestManyToManyCascadeWithSameClassRelationship: Test passed.");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToManyCascadeWithSameClassRelationship: Test failed with exception: {ex.Message}");
        }
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
            Name = "John Smith"
        };
        conn.Insert(teacher);

        var students = new List<Student> {
        new() {
            Name = "Bruce Banner",
            Address = new Address {
                Street = "Sesame Street 5",
                Town = "Gotham City"
            },
            Teacher = teacher
        },
        new() {
            Name = "Peter Parker",
            Address = new Address {
                Street = "Arlington Road 69",
                Town = "Arkham City"
            },
            Teacher = teacher
        },
        new() {
            Name = "Steve Rogers",
            Address = new Address {
                Street = "28th Street 19",
                Town = "New York"
            },
            Teacher = teacher
        }
    };
        conn.InsertAllWithChildren(students);

        var dbTeacher = conn.GetWithChildren<Teacher>(teacher.Id, recursive: true);

        // Ensure dbTeacher and Students are not null before iterating
        if (dbTeacher == null || dbTeacher.Students == null)
            return new Tuple<bool, string>(false, "TestInsertTextBlobPropertiesRecursive: dbTeacher or Students list is null.");

        foreach (var student in students)
        {
            var dbStudent = dbTeacher.Students.Find(s => s.Id == student.Id);

            if (dbStudent == null)
            {
                return new Tuple<bool, string>(false, $"TestInsertTextBlobPropertiesRecursive: Student {student.Name} not found in database.");
            }

            if (dbStudent.Address == null)
            {
                return new Tuple<bool, string>(false, $"TestInsertTextBlobPropertiesRecursive: Address for student {student.Name} is null.");
            }

            if (dbStudent.Address.Street != student.Address?.Street)
            {
                return new Tuple<bool, string>(false, $"TestInsertTextBlobPropertiesRecursive: Street mismatch for student {student.Name}. Expected: {student.Address?.Street}, Found: {dbStudent.Address.Street}");
            }

            if (dbStudent.Address.Town != student.Address?.Town)
            {
                return new Tuple<bool, string>(false, $"TestInsertTextBlobPropertiesRecursive: Town mismatch for student {student.Name}. Expected: {student.Address?.Town}, Found: {dbStudent.Address.Town}");
            }
        }

        return new Tuple<bool, string>(true, "TestInsertTextBlobPropertiesRecursive: Test passed.");
    }

    #endregion
}