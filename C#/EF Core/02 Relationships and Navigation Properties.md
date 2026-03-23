# Relationships & Navigation Properties in EF Core

---

## What Is a Navigation Property?

A **navigation property** is a property on an entity that points to a related entity (or collection of entities). It's how you "navigate" from one object to another in memory — instead of manually joining tables in SQL, you just follow the property.

```csharp
// Instead of writing:
// SELECT * FROM Orders JOIN Customers ON Orders.CustomerId = Customers.Id

// You just do:
var customerName = order.Customer.Name;  // ← navigation property
```

EF Core uses navigation properties to understand relationships and generate the correct SQL joins, foreign keys, and cascade rules automatically.

---

## One-to-Many — The Most Common Relationship

One customer has many orders. One department has many employees.

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Order> Orders { get; set; }  // ← collection navigation (the "many" side)
}

public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }

    public int CustomerId { get; set; }      // ← foreign key
    public Customer Customer { get; set; }   // ← reference navigation (the "one" side)
}
```

EF Core sees `CustomerId` + `Customer` together and figures out the relationship by convention — no configuration needed. The database gets a `CustomerId` foreign key column in the `Orders` table automatically.

### Navigation in Both Directions

```csharp
// From the "one" side → access the collection
var customer = context.Customers.Include(c => c.Orders).Find(1);
foreach (var order in customer.Orders)
    Console.WriteLine(order.Total);

// From the "many" side → access the parent
var order = context.Orders.Include(o => o.Customer).Find(5);
Console.WriteLine(order.Customer.Name);
```

---

## One-to-One — Splitting an Entity

One user has one profile. One order has one shipping address.

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }

    public UserProfile Profile { get; set; }  // ← reference navigation
}

public class UserProfile
{
    public int Id { get; set; }
    public string Bio { get; set; }
    public string AvatarUrl { get; set; }

    public int UserId { get; set; }           // ← foreign key
    public User User { get; set; }            // ← back reference
}
```

EF Core needs a little help here — it needs to know which end holds the foreign key:

```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.Profile)
    .WithOne(p => p.User)
    .HasForeignKey<UserProfile>(p => p.UserId);  // ← tell EF which side owns the FK
```

### Why Split at All?

You might not need a user's bio on every query. Splitting into a separate table means the heavy profile data is only fetched when you explicitly `Include()` it.

---

## Many-to-Many — The Join Table

Students enroll in many courses. Courses have many students.

### Modern EF Core (5+) — Skip Navigation

```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Course> Courses { get; set; }  // ← skip navigation, no join entity needed
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }

    public List<Student> Students { get; set; }
}
```

EF Core automatically creates a hidden `CourseStudent` join table with `CourseId` and `StudentId` columns. You never write it — it just exists in the database.

```csharp
// Adding a student to a course — EF handles the join table row
student.Courses.Add(course);
context.SaveChanges();
// → INSERT INTO CourseStudent (StudentsId, CoursesId) VALUES (1, 3)
```

### When You Need Payload on the Join — Explicit Join Entity

If the relationship itself has data (e.g. enrollment date, grade), you need an explicit join entity:

```csharp
public class Enrollment
{
    public int StudentId { get; set; }
    public Student Student { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public DateTime EnrolledOn { get; set; }  // ← payload data on the relationship
    public string Grade { get; set; }
}

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Enrollment> Enrollments { get; set; }  // ← through the join entity
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; }

    public List<Enrollment> Enrollments { get; set; }
}
```

```csharp
modelBuilder.Entity<Enrollment>()
    .HasKey(e => new { e.StudentId, e.CourseId });  // ← composite primary key
```

Now the join table row carries real data — grade, date — not just two foreign keys.

---

## Cascade Delete — What Happens to Children

When you delete a parent, what happens to its children?

```csharp
modelBuilder.Entity<Customer>()
    .HasMany(c => c.Orders)
    .WithOne(o => o.Customer)
    .OnDelete(DeleteBehavior.Cascade);   // ← delete customer → delete all their orders
```

The four options:

| Behavior | What Happens to Children |
|---|---|
| `Cascade` | Children are deleted automatically |
| `Restrict` | Delete is blocked if children exist |
| `SetNull` | FK on children is set to `null` |
| `NoAction` | Nothing — you handle it manually |

EF Core defaults to `Cascade` for required relationships and `SetNull` for optional ones.

---

## Fluent API — Full Control Over Relationships

Conventions handle simple cases, but Fluent API gives you precise control:

```csharp
modelBuilder.Entity<Order>()
    .HasOne(o => o.Customer)           // Order has one Customer
    .WithMany(c => c.Orders)           // Customer has many Orders
    .HasForeignKey(o => o.CustomerId)  // FK is this property
    .IsRequired()                      // FK cannot be null
    .OnDelete(DeleteBehavior.Restrict); // block delete if orders exist
```

Reading it out loud: "An Order has one Customer, a Customer has many Orders, the foreign key is CustomerId, it's required, and deleting a Customer with Orders is blocked."

---

## Shadow Foreign Keys — FK Without the Property

Sometimes you don't want a `CustomerId` property cluttering your entity. EF Core supports **shadow properties** — FK columns that exist in the database but not in your C# class:

```csharp
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    // No CustomerId property here!

    public Customer Customer { get; set; }  // just the navigation
}
```

```csharp
modelBuilder.Entity<Order>()
    .HasOne(o => o.Customer)
    .WithMany(c => c.Orders)
    .HasForeignKey("CustomerId");  // ← string name — shadow property
```

The `CustomerId` column still exists in the database. You just can't access it directly in C# — EF Core manages it behind the scenes.

---

## Self-Referencing Relationships — Hierarchies

An employee can have a manager, who is also an employee. A category can have a parent category.

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int? ParentId { get; set; }                 // ← nullable FK (root has no parent)
    public Category Parent { get; set; }               // ← reference to self
    public List<Category> SubCategories { get; set; }  // ← collection of self
}
```

```csharp
modelBuilder.Entity<Category>()
    .HasOne(c => c.Parent)
    .WithMany(c => c.SubCategories)
    .HasForeignKey(c => c.ParentId);
```

The database gets a `ParentId` column in the `Categories` table that points back to the same table — modelling a tree structure.

---

## Loading Strategies — When Does the Data Actually Load?

### Eager Loading — Load It All Upfront

```csharp
var customers = context.Customers
    .Include(c => c.Orders)
        .ThenInclude(o => o.Items)
            .ThenInclude(i => i.Product)
    .ToList();
```

One round-trip, full graph in memory. Best when you know you'll need the related data.

### Explicit Loading — Load on Demand, Controlled

```csharp
var customer = context.Customers.Find(1);
// Orders not loaded yet

// Load later, explicitly
context.Entry(customer)
    .Collection(c => c.Orders)
    .Load();

// With a filter — only load recent orders
context.Entry(customer)
    .Collection(c => c.Orders)
    .Query()
    .Where(o => o.Total > 100)
    .Load();
```

### Lazy Loading — Load When Accessed (With Caveats)

```csharp
// Requires virtual navigation properties + proxy package
public class Customer
{
    public virtual List<Order> Orders { get; set; }  // ← virtual
}
```

```csharp
var customer = context.Customers.Find(1);
var count = customer.Orders.Count;  // ← DB hit happens HERE, automatically
```

Lazy loading is convenient but dangerous. Every time you access an unloaded navigation property, EF Core fires a **separate round-trip to the database**. It hides database calls inside property access — and can trigger the **N+1 problem**.

---

## The N+1 Problem — The Hidden Performance Killer

The cost of lazy loading is an additional request to the DB each time an unloaded navigation property is accessed. If many such requests happen, performance suffers fast.

```csharp
var orders = context.Orders.ToList();  // 1 query — fetches 500 orders

foreach (var order in orders)
{
    // Lazy loading fires HERE — one DB call per order
    var name = order.Customer.Name;    // 500 separate queries
}
// Total: 501 round-trips for what could have been 1
```

The pain isn't just the number of queries — it's that each one carries **network latency**. If your DB is even 1ms away, 500 queries = 500ms of pure waiting, before any data processing even starts.

### Why It's Sneaky

The code looks completely innocent — you're just reading a property. There's no SQL anywhere in sight. The problem only shows up under load or with real data. Invisible in dev with 10 rows, catastrophic in production with 10,000.

### The Three Fixes, Ranked

**1. Eager loading — best default choice**

```csharp
var orders = context.Orders
    .Include(o => o.Customer)   // ← one JOIN query, done
    .ToList();
```

One SQL query with a JOIN. No matter how many orders, it's always 1 round-trip.

**2. Explicit loading — when you need conditional control**

```csharp
var order = context.Orders.Find(id);

// Only load the customer if actually needed
if (needsCustomerDetails)
    context.Entry(order).Reference(o => o.Customer).Load();
```

You control exactly when the DB hit happens — useful when the related data is only needed in some code paths.

**3. Projections — the most efficient, take only what you need**

```csharp
var result = context.Orders
    .Select(o => new {
        o.Id,
        o.Total,
        CustomerName = o.Customer.Name   // ← EF generates a JOIN, no lazy load
    })
    .ToList();
```

EF Core translates this into a single query fetching only the columns you need — no full entity materialisation at all. Best for read-heavy scenarios like dashboards.

The rule of thumb: **if you're going to loop over it, always `Include()` it first**. Lazy loading is fine for one-off lookups, but never inside a loop.

---

## Summary

```
One-to-Many:    FK on the "many" side, collection navigation on "one" side
One-to-One:     FK on the dependent side, tell EF with HasForeignKey<T>
Many-to-Many:   EF creates hidden join table, or use explicit join entity for payload
Cascade:        Control what happens to children when parent is deleted
Loading:        Eager (Include) = safe, Explicit = controlled, Lazy = convenient but risky
N+1:            Always Include() what you'll navigate — never let lazy loading loop
```
