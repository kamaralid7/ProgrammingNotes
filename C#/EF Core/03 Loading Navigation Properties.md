# Loading Navigation Properties — Include, Projection & Explicit Loading

---

## The Three Ways to Load Related Data

When EF Core loads an entity, navigation properties are **null by default**. You have three tools to bring them to life, each suited to different situations.

---

## `Include()` — Eager Loading

You tell EF Core upfront exactly what related data you want. It generates a single SQL query with JOINs and hands you the fully populated object graph.

```csharp
var customers = context.Customers
    .Include(c => c.Orders)
    .ToList();
```

Generated SQL:
```sql
SELECT c.*, o.*
FROM Customers c
LEFT JOIN Orders o ON o.CustomerId = c.Id
```

One trip to the database. Every customer has their `Orders` collection populated.

---

### `ThenInclude()` — Going Deeper

When you need to go beyond the first level of the graph:

```csharp
var customers = context.Customers
    .Include(c => c.Orders)
        .ThenInclude(o => o.Items)
            .ThenInclude(i => i.Product)
    .ToList();
```

Each `.ThenInclude()` steps one level deeper into the graph. Now `customer.Orders[0].Items[0].Product.Name` is fully available — no extra queries.

---

### Multiple Independent Includes

You can include siblings — multiple independent navigation properties at the same level:

```csharp
var customers = context.Customers
    .Include(c => c.Orders)          // ← branch 1
    .Include(c => c.Addresses)       // ← branch 2 — independent
    .Include(c => c.PaymentMethods)  // ← branch 3 — independent
    .ToList();
```

EF Core generates one query per branch (or a single split query — more on that below).

---

### Filtered Include — EF Core 5+

You can filter the included collection, not just load all of it:

```csharp
var customers = context.Customers
    .Include(c => c.Orders
        .Where(o => o.Total > 100)        // ← only include qualifying orders
        .OrderByDescending(o => o.Date)   // ← and sort them
        .Take(5))                         // ← only the top 5
    .ToList();
```

Before EF Core 5, this required a separate query. Now it folds right into the `Include`. The filter runs in SQL, not in memory.

---

### Split Queries — When One Big JOIN Gets Too Wide

When you `Include` multiple collections, EF Core generates a single query with multiple JOINs. This can produce a **cartesian explosion** — if a customer has 10 orders and 10 addresses, the join produces 100 rows (10 × 10) just to represent 20 pieces of data.

```csharp
var customers = context.Customers
    .Include(c => c.Orders)
    .Include(c => c.Addresses)
    .AsSplitQuery()   // ← tells EF Core to use multiple queries instead
    .ToList();
```

With `AsSplitQuery()`, EF Core runs 3 separate focused queries:

```sql
SELECT * FROM Customers;
SELECT * FROM Orders WHERE CustomerId IN (...);
SELECT * FROM Addresses WHERE CustomerId IN (...);
```

Much less data transferred. The tradeoff: 3 round-trips instead of 1, and they aren't in a transaction by default (a row could change between queries). Use it when you're seeing performance issues from wide joins.

You can also make split query the default for the entire context:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlServer("...")
              .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
```

---

## Projection — `Select()` — Take Only What You Need

`Include()` loads full entity objects. Projection lets you shape the output into exactly what you need — like a custom DTO (Data Transfer Object) — and EF Core generates optimised SQL that fetches only those columns.

```csharp
var result = context.Orders
    .Select(o => new
    {
        o.Id,
        o.Total,
        CustomerName = o.Customer.Name,    // ← EF generates a JOIN automatically
        ItemCount    = o.Items.Count       // ← EF generates COUNT(*) in SQL
    })
    .ToList();
```

Generated SQL:
```sql
SELECT o.Id, o.Total, c.Name, COUNT(i.Id)
FROM Orders o
INNER JOIN Customers c ON c.Id = o.CustomerId
LEFT JOIN OrderItems i ON i.OrderId = o.Id
GROUP BY o.Id, o.Total, c.Name
```

You didn't write a single JOIN or COUNT. EF Core translated the C# shape into optimal SQL.

---

### Projecting Into a Named DTO

Anonymous types are fine for internal use, but for API responses or layered apps you'll usually project into a named class:

```csharp
public class OrderSummary
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public string CustomerName { get; set; }
    public int ItemCount { get; set; }
}

var summaries = context.Orders
    .Select(o => new OrderSummary
    {
        Id           = o.Id,
        Total        = o.Total,
        CustomerName = o.Customer.Name,
        ItemCount    = o.Items.Count
    })
    .ToList();
```

---

### Why Projection Beats `Include()` for Read-Only Data

```csharp
// Include — fetches entire Customer and Order entities (all columns)
var orders = context.Orders
    .Include(o => o.Customer)
    .ToList();

// Projection — fetches ONLY the 4 columns you need
var orders = context.Orders
    .Select(o => new { o.Id, o.Total, o.Customer.Name, o.Customer.Email })
    .ToList();
```

With `Include`, if `Customer` has 20 columns, all 20 come back from the DB. Projection surgically fetches only what you asked for. For dashboards, API endpoints, or reports — always prefer projection.

The caveat: projected results are **not tracked** by EF Core. You can't modify them and call `SaveChanges()`. Projection is read-only by nature.

---

## Explicit Loading — On-Demand, Surgical

Explicit loading is the middle ground: you load the root entity first, then decide later whether and what to load — with full control over each navigation property.

```csharp
var customer = context.Customers.Find(1);
// At this point: customer.Orders is null

// Later, when you actually need it
await context.Entry(customer)
    .Collection(c => c.Orders)   // ← for collection navigations
    .LoadAsync();

// For a reference navigation (single object, not a list)
await context.Entry(order)
    .Reference(o => o.Customer)  // ← for reference navigations
    .LoadAsync();
```

---

### Explicit Loading With a Filter

The real power of explicit loading is you can filter what you load:

```csharp
var customer = context.Customers.Find(1);

// Load only unpaid orders
await context.Entry(customer)
    .Collection(c => c.Orders)
    .Query()                              // ← drop into queryable
    .Where(o => o.Status == "Unpaid")
    .OrderBy(o => o.DueDate)
    .LoadAsync();

// customer.Orders now contains only unpaid orders — not all of them
```

---

### Checking If Already Loaded

```csharp
var entry = context.Entry(customer);

if (!entry.Collection(c => c.Orders).IsLoaded)
{
    await entry.Collection(c => c.Orders).LoadAsync();
}
```

Avoids a redundant DB call if the navigation was already populated earlier in the same request.

---

## Comparing All Three

| | `Include()` | Projection `Select()` | Explicit Loading |
|---|---|---|---|
| When data loads | At query time | At query time | You decide |
| Entities tracked | Yes | No | Yes |
| Fetch only needed columns | No (full entities) | Yes | No (full entities) |
| Filter included data | Yes (EF Core 5+) | Yes | Yes (via `.Query()`) |
| Best for | Saving/updating related data | Read-only, API responses | Conditional loading logic |
| SQL generated | Single JOIN or split queries | Optimised per shape | One query per `.Load()` call |

---

## The Golden Rules

Use `Include()` when you're going to **modify** the related data and call `SaveChanges()`.

Use `Select()` projection when you're **reading only** — APIs, reports, dashboards. It's always faster because it fetches fewer columns and skips entity materialisation.

Use explicit loading when the decision of **what to load depends on runtime conditions** — checking a flag, user role, or result of a previous query.

Never rely on lazy loading inside loops — it's the N+1 problem waiting to happen.
