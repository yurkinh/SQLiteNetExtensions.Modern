using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using IntegratedTestsSampleApp.Helpers;

namespace IntegratedTestsSampleApp.Tests;

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

    public static async Task<Tuple<bool, string>> TestOneToOneCascadeWithInverseAsync()
    {
        try
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
            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: obtainedPerson is null");
            if (obtainedPerson.Passport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: obtainedPerson.Passport is null");
            if (obtainedPerson.Passport.Owner == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: Circular reference not resolved");
            if (obtainedPerson.Identifier != obtainedPerson.Passport.Owner.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: Identifier mismatch");
            if (obtainedPerson.Passport.Id != obtainedPerson.Passport.Owner.Passport.Id)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: Passport ID mismatch");

            var obtainedPassport = await conn.GetWithChildrenAsync<PassportWithForeignKey>(passport.Id, recursive: true);
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: obtainedPassport is null");
            if (obtainedPassport.Owner == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: obtainedPassport.Owner is null");
            if (obtainedPassport.Owner.Passport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: Circular reference not resolved in passport");
            if (obtainedPassport.Id != obtainedPassport.Owner.Passport.Id)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseAsync: Passport ID mismatch in owner");

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseAsync: Exception occurred - {ex.Message}");
        }
    }

    /// <summary>
    /// Same test that TestOneToOneCascadeWithInverse but fetching the passport instead of the person
    /// </summary>

    public static async Task<Tuple<bool, string>> TestOneToOneCascadeWithInverseReversedAsync()
    {
        try
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
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversedAsync: obtainedPassport is null");
            if (obtainedPassport.Owner == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversedAsync: obtainedPassport.Owner is null");
            if (obtainedPassport.Owner.Passport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversedAsync: Circular reference not resolved");
            if (obtainedPassport.Id != obtainedPassport.Owner.Passport.Id)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversedAsync: Passport ID mismatch");
            if (obtainedPassport.Owner.Identifier != obtainedPassport.Owner.Passport.Owner.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseReversedAsync: Owner Identifier mismatch");

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseReversedAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseReversedAsync: Exception occurred - {ex.Message}");
        }
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

    public static async Task<Tuple<bool, string>> TestOneToOneCascadeWithInverseDoubleForeignKeyAsync()
    {
        try
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
            if (obtainedPerson == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: obtainedPerson is null");
            if (obtainedPerson.Passport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: obtainedPerson.Passport is null");
            if (obtainedPerson.Passport.Owner == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: Circular reference not resolved");
            if (obtainedPerson.Identifier != obtainedPerson.Passport.Owner.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: Identifier mismatch");
            if (obtainedPerson.Passport.Id != obtainedPerson.Passport.Owner.Passport.Id)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: Passport ID mismatch");

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseDoubleForeignKeyAsync: Exception occurred - {ex.Message}");
        }
    }

    /// <summary>
    /// Same test that TestOneToOneCascadeWithInverseDoubleForeignKey but fetching the passport instead of the person
    /// </summary>

    public static async Task<Tuple<bool, string>> TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync()
    {
        try
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
            if (obtainedPassport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: obtainedPassport is null");
            if (obtainedPassport.Owner == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: obtainedPassport.Owner is null");
            if (obtainedPassport.Owner.Passport == null)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: Circular reference not resolved");
            if (obtainedPassport.Id != obtainedPassport.Owner.Passport.Id)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: Passport ID mismatch");
            if (obtainedPassport.Owner.Identifier != obtainedPassport.Owner.Passport.Owner.Identifier)
                return new Tuple<bool, string>(false, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: Owner Identifier mismatch");

            return new Tuple<bool, string>(true, "TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToOneCascadeWithInverseDoubleForeignKeyReversedAsync: Exception occurred - {ex.Message}");
        }
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

    public static async Task<Tuple<bool, string>> TestOneToManyCascadeWithInverseAsync()
    {
        try
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
            new Order { Amount = 98f, Date = new DateTime(2014, 2, 1, 22, 31, 7) }
        };

            await conn.InsertAsync(customer);
            await conn.InsertAllAsync(orders);

            customer.Orders = orders;
            await conn.UpdateWithChildrenAsync(customer);

            var expectedOrders = orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<Customer>(customer.Id, recursive: true);
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: obtainedCustomer is null");

            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: obtainedCustomer.Orders is null");

            if (expectedOrders.Count != obtainedCustomer.Orders.Length)
                return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: Unexpected order ID");

                var expectedOrder = expectedOrders[order.Id];

                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestOneToManyCascadeWithInverseAsync: Order amount mismatch for Order ID {order.Id}");

                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestOneToManyCascadeWithInverseAsync: Order date mismatch for Order ID {order.Id}");

                if (order.Customer == null)
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: order.Customer is null");

                if (order.Customer.Id != customer.Id)
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: Customer ID mismatch");

                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: Customer name mismatch");

                if (order.Customer.Orders == null)
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: order.Customer.Orders is null");

                if (expectedOrders.Count != order.Customer.Orders.Length)
                    return new Tuple<bool, string>(false, "TestOneToManyCascadeWithInverseAsync: Nested orders count mismatch");
            }

            return new Tuple<bool, string>(true, "TestOneToManyCascadeWithInverseAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestOneToManyCascadeWithInverseAsync: Exception occurred - {ex.Message}");
        }
    }

    #endregion

    #region ManyToOneCascadeWithInverseAsync
    /// <summary>
    /// In this test we will execute the same test that we did in TestOneToManyCascadeWithInverse but fetching
    /// one of the orders
    /// </summary>

    public static async Task<Tuple<bool, string>> TestManyToOneCascadeWithInverseAsync()
    {
        try
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
            new Order { Amount = 98f, Date = new DateTime(2014, 2, 1, 22, 31, 7) }
        };

            await conn.InsertAsync(customer);
            await conn.InsertAllAsync(orders);

            customer.Orders = orders;
            await conn.UpdateWithChildrenAsync(customer);

            var orderToFetch = orders[2];
            var expectedOrders = orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedOrder = await conn.GetWithChildrenAsync<Order>(orderToFetch.Id, recursive: true);
            if (obtainedOrder == null)
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverseAsync: obtainedOrder is null");

            if (orderToFetch.Date != obtainedOrder.Date)
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverseAsync: Order date mismatch");

            if (Math.Abs(orderToFetch.Amount - obtainedOrder.Amount) > 0.0001)
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverseAsync: Order amount mismatch");

            var obtainedCustomer = obtainedOrder.Customer;
            if (obtainedCustomer == null)
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverseAsync: obtainedCustomer is null");

            if (obtainedCustomer.Orders == null)
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverseAsync: obtainedCustomer.Orders is null");

            if (expectedOrders.Count != obtainedCustomer.Orders.Length)
                return new Tuple<bool, string>(false, "TestManyToOneCascadeWithInverseAsync: Orders count mismatch");

            foreach (var order in obtainedCustomer.Orders)
            {
                if (!expectedOrders.ContainsKey(order.Id))
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Unexpected order ID {order.Id}");

                var expectedOrder = expectedOrders[order.Id];

                if (Math.Abs(expectedOrder.Amount - order.Amount) > 0.0001)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Order amount mismatch for Order ID {order.Id}");

                if (expectedOrder.Date != order.Date)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Order date mismatch for Order ID {order.Id}");

                if (order.Customer == null)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: order.Customer is null for Order ID {order.Id}");

                if (order.Customer.Id != customer.Id)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Customer ID mismatch for Order ID {order.Id}");

                if (order.Customer.Name != customer.Name)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Customer name mismatch for Order ID {order.Id}");

                if (order.Customer.Orders == null)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: order.Customer.Orders is null for Order ID {order.Id}");

                if (expectedOrders.Count != order.Customer.Orders.Length)
                    return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Nested orders count mismatch for Order ID {order.Id}");
            }

            return new Tuple<bool, string>(true, "TestManyToOneCascadeWithInverseAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"TestManyToOneCascadeWithInverseAsync: Exception occurred - {ex.Message}");
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

    public static async Task<Tuple<bool, string>> TestManyToManyCascadeWithSameClassRelationshipAsync()
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

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };
            await conn.InsertAllAsync(allUsers);

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
                await conn.UpdateWithChildrenAsync(user);
            }

            Func<TwitterUser, TwitterUser, string> checkUser = (expected, obtained) =>
            {
                if (obtained == null)
                    return $"User is null: {expected.Name}";

                if (expected.Name != obtained.Name)
                    return $"Expected name '{expected.Name}' but got '{obtained.Name}'";

                if (!expected.FollowingUsers.OrderBy(u => u.Name).SequenceEqual(obtained.FollowingUsers.OrderBy(u => u.Name)))
                    return $"Following users mismatch for '{expected.Name}'";

                var followers = allUsers.Where(u => u.FollowingUsers.Contains(expected)).OrderBy(u => u.Name);
                if (!followers.SequenceEqual(obtained.Followers.OrderBy(u => u.Name)))
                    return $"Followers mismatch for '{expected.Name}'";

                return null;
            };

            var obtainedThomas = await conn.GetWithChildrenAsync<TwitterUser>(thomas.Id, recursive: true);
            var error = checkUser(thomas, obtainedThomas);
            if (error != null) return new Tuple<bool, string>(false, error);

            var obtainedJohn = obtainedThomas.FollowingUsers.FirstOrDefault(u => u.Id == john.Id);
            error = checkUser(john, obtainedJohn);
            if (error != null) return new Tuple<bool, string>(false, error);

            var obtainedPeter = obtainedJohn.FollowingUsers.FirstOrDefault(u => u.Id == peter.Id);
            error = checkUser(peter, obtainedPeter);
            if (error != null) return new Tuple<bool, string>(false, error);

            var obtainedMartha = obtainedPeter.FollowingUsers.FirstOrDefault(u => u.Id == martha.Id);
            error = checkUser(martha, obtainedMartha);
            if (error != null) return new Tuple<bool, string>(false, error);

            var obtainedAnthony = obtainedMartha.FollowingUsers.FirstOrDefault(u => u.Id == anthony.Id);
            error = checkUser(anthony, obtainedAnthony);
            if (error != null) return new Tuple<bool, string>(false, error);

            var obtainedJaime = obtainedThomas.Followers.FirstOrDefault(u => u.Id == jaime.Id);
            error = checkUser(jaime, obtainedJaime);
            if (error != null) return new Tuple<bool, string>(false, error);

            var obtainedMark = obtainedJaime.FollowingUsers.FirstOrDefault(u => u.Id == mark.Id);
            error = checkUser(mark, obtainedMark);
            if (error != null) return new Tuple<bool, string>(false, error);

            return new Tuple<bool, string>(true, "TestManyToManyCascadeWithSameClassRelationshipAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"Exception occurred: {ex.Message}");
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
                Name = "John Smith"
            };
            await conn.InsertAsync(teacher);

            var students = new List<Student>
        {
            new Student
            {
                Name = "Bruce Banner",
                Address = new Address
                {
                    Street = "Sesame Street 5",
                    Town = "Gotham City"
                },
                Teacher = teacher
            },
            new Student
            {
                Name = "Peter Parker",
                Address = new Address
                {
                    Street = "Arlington Road 69",
                    Town = "Arkham City"
                },
                Teacher = teacher
            },
            new Student
            {
                Name = "Steve Rogers",
                Address = new Address
                {
                    Street = "28th Street 19",
                    Town = "New York"
                },
                Teacher = teacher
            }
        };

            await conn.InsertAllWithChildrenAsync(students);

            var dbTeacher = await conn.GetWithChildrenAsync<Teacher>(teacher.Id, recursive: true);
            if (dbTeacher == null)
                return new Tuple<bool, string>(false, "Teacher was not found in the database.");

            foreach (var student in students)
            {
                var dbStudent = dbTeacher.Students.Find(s => s.Id == student.Id);
                if (dbStudent == null)
                    return new Tuple<bool, string>(false, $"Student '{student.Name}' not found.");

                if (dbStudent.Address == null)
                    return new Tuple<bool, string>(false, $"Address for student '{student.Name}' is null.");

                if (dbStudent.Address.Street != student.Address.Street)
                    return new Tuple<bool, string>(false, $"Street mismatch for '{student.Name}': Expected '{student.Address.Street}', got '{dbStudent.Address.Street}'.");

                if (dbStudent.Address.Town != student.Address.Town)
                    return new Tuple<bool, string>(false, $"Town mismatch for '{student.Name}': Expected '{student.Address.Town}', got '{dbStudent.Address.Town}'.");
            }

            return new Tuple<bool, string>(true, "TestInsertTextBlobPropertiesRecursiveAsync: Passed");
        }
        catch (Exception ex)
        {
            return new Tuple<bool, string>(false, $"Exception occurred: {ex.Message}");
        }
    }

    #endregion
}
