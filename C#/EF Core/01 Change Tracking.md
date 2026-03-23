# Change Tracking in EF Core

Change tracking is EF Core's internal system that watches your entities and figures out what SQL to run when you call `SaveChanges()`.

---

## The Mental Model: A Snapshot in Memory

When EF Core loads an entity from the database, it keeps **two copies** in memory:

- The **original values** (what came from the DB)
- The **current values** (what your code has now)

When you call `SaveChanges()`, it diffs the two. If anything changed → generate SQL.

---

## Entity States — The Heart of Change Tracking

Every entity tracked by EF Core has a **state**:

| State | Meaning | SQL Generated |
|---|---|---|
| `Detached` | EF Core doesn't know about it | None |
| `Unchanged` | Loaded, but nothing changed | None |
| `Added` | New, not yet in DB | `INSERT` |
| `Modified` | Loaded and changed | `UPDATE` |
| `Deleted` | Marked for removal | `DELETE` |

You can inspect or manually set these:

```csharp
var product = context.Products.Find(1);

// Check the state
var state = context.Entry(product).State;
// → EntityState.Unchanged

product.Price = 149.99m;

// After change
var newState = context.Entry(product).State;
// → EntityState.Modified  ← EF Core noticed the change automatically
```

---

## How EF Core Detects Changes

### Default: Snapshot Change Tracking

When you load an entity, EF Core takes a **snapshot** of all its property values. When you call `SaveChanges()` (or explicitly call `ChangeTracker.DetectChanges()`), it compares current values to the snapshot.

```csharp
var order = context.Orders.Find(42);
// Snapshot: { Status = "Pending", Total = 200.00 }

order.Status = "Shipped";
order.Total = 215.00m;

context.SaveChanges();
// Diff found → generates:
// UPDATE Orders SET Status='Shipped', Total=215.00 WHERE Id=42
```

Only the **changed columns** are updated — not all of them.

---

### Faster Alternative: Proxies with `INotifyPropertyChanged`

EF Core can use **proxy classes** — auto-generated subclasses that raise property-change events the moment something changes.

```csharp
// Your entity must use virtual properties for proxies to work
public class Product
{
    public int Id { get; set; }
    public virtual string Name { get; set; }   // virtual ← required
    public virtual decimal Price { get; set; }
}
```

```csharp
// Enable in DbContext
protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseLazyLoadingProxies()
              .UseSqlServer("...");
```

The proxy intercepts every property set and immediately marks the entity as `Modified` — no diff needed at save time.

---

## The `ChangeTracker` API — Looking Under the Hood

```csharp
// See everything EF Core is currently tracking
foreach (var entry in context.ChangeTracker.Entries())
{
    Console.WriteLine($"{entry.Entity.GetType().Name}: {entry.State}");
}

// See original vs current values
var entry = context.Entry(product);
var originalPrice = entry.Property(p => p.Price).OriginalValue;  // 99.99
var currentPrice  = entry.Property(p => p.Price).CurrentValue;   // 149.99
```

---

## Practical Pattern: Audit Log with Change Tracking

```csharp
public override int SaveChanges()
{
    var changes = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified);

    foreach (var entry in changes)
    {
        foreach (var prop in entry.Properties)
        {
            if (prop.IsModified)
            {
                Console.WriteLine(
                    $"{entry.Entity.GetType().Name}.{prop.Metadata.Name}: " +
                    $"{prop.OriginalValue} → {prop.CurrentValue}");
            }
        }
    }

    return base.SaveChanges();
}
```

**Output:**
```
Product.Price: 99.99 → 149.99
Product.Name: Widget → Super Widget
```

---

## Detached Entities — The "Lost" Case

If an entity is created **outside** the context (e.g. from an API request body), EF Core has no snapshot for it:

```csharp
var incomingProduct = new Product { Id = 5, Name = "Updated", Price = 79.99m };

context.Entry(incomingProduct).State = EntityState.Modified;
// EF Core will generate UPDATE for ALL properties (no snapshot = full update)

context.SaveChanges();
```

Since there's no snapshot, EF Core updates **every column** — not just the changed ones.

---

## `AsNoTracking()` — Opt Out When You Don't Need It

For read-only queries (dashboards, reports), tracking is pure overhead:

```csharp
var products = context.Products
    .AsNoTracking()    // ← no snapshot stored, no state tracking
    .Where(p => p.Price > 100)
    .ToList();
```

Significantly faster for large result sets.

---

## The C# 12 Angle — `required` and `init` Properties

```csharp
public class Customer
{
    public int Id { get; set; }
    public required string Email { get; init; }  // init-only!
    public string Name { get; set; }
}
```

EF Core **can still track** `init`-only properties because it uses reflection internally during materialization. At the application level, `init` prevents accidental mutation — if you can't change it in code, EF Core won't see it as `Modified` either.

---

# Concurrency Conflicts — When Two Users Touch the Same Row

## The Real-World Problem

Two agents pull up the same flight with 1 seat left. Both click "Book" at the same time. Without concurrency handling, both succeed — you've sold 2 tickets for 1 seat. This is called a **lost update**.

---

## Optimistic Concurrency (EF Core's Default Approach)

Assume conflicts are rare, and only check at save time. No database locks are held while the user is working.

EF Core uses a **concurrency token** — a value that changes every time the row is updated.

### Using `RowVersion`

```csharp
public class Flight
{
    public int Id { get; set; }
    public string Route { get; set; }
    public int AvailableSeats { get; set; }

    [Timestamp]                          // ← concurrency token
    public byte[] RowVersion { get; set; }
}
```

### What happens under the hood:

```
Agent A loads flight → RowVersion = 0x0001
Agent B loads flight → RowVersion = 0x0001

Agent A saves: AvailableSeats = 0
  → UPDATE Flights SET AvailableSeats=0 WHERE Id=1 AND RowVersion=0x0001
  → Success! RowVersion becomes 0x0002

Agent B saves: AvailableSeats = 0
  → UPDATE Flights SET AvailableSeats=0 WHERE Id=1 AND RowVersion=0x0001
  → 0 rows affected — RowVersion is now 0x0002, not 0x0001!
  → EF Core throws DbUpdateConcurrencyException
```

### Handling the Conflict

```csharp
try
{
    context.SaveChanges();
}
catch (DbUpdateConcurrencyException ex)
{
    var entry = ex.Entries.Single();

    var dbValues       = await entry.GetDatabaseValuesAsync(); // what's in DB now
    var originalValues = entry.OriginalValues;                 // what Agent B loaded
    var currentValues  = entry.CurrentValues;                  // what Agent B tried to save

    // Strategy 1: "Database wins" — discard Agent B's changes
    entry.OriginalValues.SetValues(dbValues);

    // Strategy 2: "Client wins" — force Agent B's version
    entry.OriginalValues.SetValues(dbValues);
    context.SaveChanges();  // retries with new RowVersion

    // Strategy 3: "Merge" — combine both changes (custom domain logic)
}
```

---

### Using `[ConcurrencyCheck]` Without RowVersion

```csharp
public class BankAccount
{
    public int Id { get; set; }

    [ConcurrencyCheck]           // ← include in WHERE clause
    public decimal Balance { get; set; }

    public string Owner { get; set; }
}
```

### Fluent API Alternative

```csharp
modelBuilder.Entity<Flight>()
    .Property(f => f.RowVersion)
    .IsRowVersion();

modelBuilder.Entity<BankAccount>()
    .Property(b => b.Balance)
    .IsConcurrencyToken();
```

---

## Pessimistic Concurrency — Locking Rows

EF Core doesn't have built-in pessimistic locking, but you can use raw SQL:

```csharp
// SQL Server: lock the row until the transaction completes
var flight = context.Flights
    .FromSqlRaw("SELECT * FROM Flights WITH (UPDLOCK) WHERE Id = {0}", flightId)
    .Single();
```

Use sparingly — can cause **deadlocks** if two transactions lock rows in different orders.

---

# Owned Entities & Value Objects in Change Tracking

## The Concept

A **value object** is defined by its values, not its identity. An `Address` is a value object — two addresses with the same street/city/zip are the same address. A `Customer` is an entity — two customers with the same name are still different people.

---

## Owned Types in EF Core

```csharp
public class Address       // ← no Id property
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address ShippingAddress { get; set; }  // owned
    public Address BillingAddress { get; set; }   // owned
}
```

```csharp
modelBuilder.Entity<Customer>()
    .OwnsOne(c => c.ShippingAddress);
modelBuilder.Entity<Customer>()
    .OwnsOne(c => c.BillingAddress);
```

### The actual database table:

| Id | Name | ShippingAddress_Street | ShippingAddress_City | BillingAddress_Street | ... |
|---|---|---|---|---|---|
| 1 | Ali | 123 Main St | Boston | 456 Oak Ave | ... |

One table, no joins. Columns are prefixed with the navigation property name.

---

## How Change Tracking Handles Owned Types

```csharp
var customer = context.Customers.Find(1);

// Replace the entire address (value object semantics)
customer.ShippingAddress = new Address
{
    Street = "789 New St",
    City = "Seattle",
    ZipCode = "98101"
};

context.SaveChanges();
// → UPDATE Customers
//   SET ShippingAddress_Street='789 New St',
//       ShippingAddress_City='Seattle',
//       ShippingAddress_ZipCode='98101'
//   WHERE Id=1
```

EF Core also tracks **partial changes**:

```csharp
customer.ShippingAddress.City = "Portland";
context.SaveChanges();
// → UPDATE Customers SET ShippingAddress_City='Portland' WHERE Id=1
```

---

## Owned Collections — Value Objects That Repeat

```csharp
public class PhoneNumber
{
    public string Type { get; set; }    // "Home", "Work", "Mobile"
    public string Number { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<PhoneNumber> PhoneNumbers { get; set; }
}
```

```csharp
modelBuilder.Entity<Customer>()
    .OwnsMany(c => c.PhoneNumbers);
```

`OwnsMany` creates a separate table (can't flatten a collection). Phone numbers have no independent identity — if the customer is deleted, they vanish too. **Cascade by design.**

---

## C# 12 Records as Value Objects

`record` types are a natural fit — they have value equality built in:

```csharp
public record Address(string Street, string City, string ZipCode);

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required Address ShippingAddress { get; set; }
}
```

Records give you `Equals()`, `GetHashCode()`, and immutability. EF Core works with records as owned types using constructor binding to materialize them from the database.

---

## Summary

```
Change Tracking:   EF Core diffs original vs current values → generates minimal SQL

Concurrency:       Who wins when two people change the same thing?
                   → RowVersion / ConcurrencyToken → detect conflict at save time

Owned Types:       Things without their own identity live inside their parent
                   → Tracked together → replace whole object or change a part
```

---

## Change Tracking Flow

```
Load entity → EF takes snapshot → You modify properties
     ↓
SaveChanges() → DetectChanges() runs → Diff snapshot vs current
     ↓
Generate minimal SQL → Execute → Clear snapshot → Mark Unchanged
```

---

# `INotifyPropertyChanged`, `INotifyPropertyChanging` & Change Tracking Strategies

## The Default Problem: Snapshot is Expensive

By default, EF Core stores a snapshot of every property of every tracked entity. At `SaveChanges()`, it loops through all of them and compares. For 10,000 tracked entities with 20 properties each — that's 200,000 comparisons on every save. This is where notification-based tracking comes in.

---

## `INotifyPropertyChanged` — "Tell Me When You're Done Changing"

This interface makes your entity **announce** when a property has already changed. EF Core listens and immediately marks that property as modified — no snapshot comparison needed at save time.

```csharp
public class Product : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string _name;
    private decimal _price;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            //                          ↑ fires AFTER the value is already set
        }
    }

    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Price)));
        }
    }
}
```

When `Name` is set, EF Core's listener fires immediately and records: "Name on entity #5 is now dirty." No full scan at save time.

---

## `INotifyPropertyChanging` — "Tell Me Before You Change"

This is the **before** counterpart. EF Core uses it to capture the **original value** at the moment just before the change — so it doesn't need a full upfront snapshot.

```csharp
public class Product : INotifyPropertyChanged, INotifyPropertyChanging
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event PropertyChangingEventHandler PropertyChanging;  // ← fires BEFORE

    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            // Fire BEFORE — EF Core captures original value here
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));

            _name = value;

            // Fire AFTER — EF Core marks property as modified
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
```

The sequence EF Core sees:

```
PropertyChanging fires → EF records original value ("Widget")
   ↓
value is set to "Super Widget"
   ↓
PropertyChanged fires → EF marks Name as Modified
   ↓
SaveChanges() → no scan needed, EF already knows exactly what changed
```

This is the most efficient mode — no snapshot at all, changes tracked in real time.

---

## How Changes Are Compared — `ValueComparer<T>`

For simple types like `string` and `int`, EF Core uses standard equality (`==`). But for complex types — arrays, JSON, custom structs — equality isn't obvious. That's where `ValueComparer<T>` comes in.

### The Problem with Arrays

```csharp
public class UserProfile
{
    public int Id { get; set; }
    public string[] Tags { get; set; }  // ← array
}
```

By default, EF Core compares arrays by **reference** — two arrays with identical contents are considered different objects. This means EF Core will always think `Tags` is modified, even if nothing changed.

### Custom `ValueComparer`

```csharp
var tagsComparer = new ValueComparer<string[]>(
    (a, b) => a.SequenceEqual(b),                                            // equality
    v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),     // hash
    v => v.ToArray()                                                          // snapshot (deep copy)
);

modelBuilder.Entity<UserProfile>()
    .Property(u => u.Tags)
    .HasConversion(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.None))
    .Metadata.SetValueComparer(tagsComparer);
```

Now EF Core correctly compares `["csharp", "dotnet"]` with `["csharp", "dotnet"]` as equal — no false `Modified` state.

The three arguments to `ValueComparer<T>` always mean:

1. **Equals** — how to compare two values
2. **HashCode** — for internal dictionaries
3. **Snapshot** — how to make a deep copy (if you shallow copy a mutable type, the snapshot changes along with the live value)

---

## `HasChangeTrackingStrategy` — Setting the Strategy Globally

Instead of implementing interfaces on every entity, you can declare a **strategy** in `ModelBuilder` that tells EF Core how to track changes for that entity (or all entities).

```csharp
// Per entity
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
}

// Globally for all entities
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasChangeTrackingStrategy(
        ChangeTrackingStrategy.ChangingAndChangedNotifications);
}
```

---

## The Four Strategies — Side by Side

### `Snapshot` (default)

```csharp
.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot)
```

- EF Core stores a full copy of every property on load
- Compares at `SaveChanges()` time
- **No interface needed** on your entity
- Slowest for large graphs, but zero boilerplate

---

### `ChangedNotifications`

```csharp
.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)
```

- Entity must implement `INotifyPropertyChanged`
- EF Core listens and marks properties dirty in real time
- No snapshot needed → faster `SaveChanges()`
- **Downside:** EF Core has no original values — so it updates the column but doesn't know the old value (relevant for audit logs)

---

### `ChangingAndChangedNotifications`

```csharp
.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications)
```

- Entity must implement **both** `INotifyPropertyChanging` and `INotifyPropertyChanged`
- EF Core captures original value before change, marks dirty after
- Most efficient — no snapshot at all, original values preserved
- Best for performance-sensitive apps with audit requirements

---

### `ChangingAndChangedNotificationsWithOriginalValues`

```csharp
.HasChangeTrackingStrategy(
    ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
```

- Like above, but EF Core **also** stores a snapshot alongside notifications
- Use when you need both original values AND real-time notifications
- Most memory usage, most flexibility

---

## Comparison Table

| Strategy | Interface Needed | Snapshot Stored | Original Values | Performance |
|---|---|---|---|---|
| `Snapshot` | None | Yes | Yes | Baseline |
| `ChangedNotifications` | `INotifyPropertyChanged` | No | No | Better |
| `ChangingAndChangedNotifications` | Both interfaces | No | Yes | Best |
| `ChangingAndChangedNotificationsWithOriginalValues` | Both interfaces | Yes | Yes | Most memory |

---

## Clean Base Class Pattern

Instead of duplicating notification boilerplate in every entity, extract it into a base class:

```csharp
public abstract class TrackedEntity : INotifyPropertyChanging, INotifyPropertyChanged
{
    public event PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

// Entities stay clean — one line per property
public class Product : TrackedEntity
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);  // ← handles both events + equality check
    }
}
```

`[CallerMemberName]` is a C# compiler attribute — it automatically fills in the calling property name at compile time so you never hardcode `"Name"` as a string.

---

## When to Use What

For most apps — use `Snapshot`. It's simple and works without touching your entities.

For high-throughput apps tracking thousands of entities — implement both notification interfaces and use `ChangingAndChangedNotifications`. The boilerplate pays off at scale.
