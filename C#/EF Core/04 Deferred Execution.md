# Deferred Execution in EF Core

---

## The Core Idea

When you write a LINQ query in EF Core, **nothing happens immediately**. You're not talking to the database yet — you're building up a description of what you *want*. The actual SQL only fires at the moment you ask for the results.

Think of it like ordering at a restaurant. Writing the LINQ query is writing your order on a notepad. The food doesn't start cooking until you hand it to the kitchen — that's the execution trigger.

---

## Building the Query vs Executing the Query

```csharp
// Step 1 — just building the query, zero DB contact
IQueryable<Product> query = context.Products
    .Where(p => p.Price > 50);

// Step 2 — still just building, adding more conditions
query = query.Where(p => p.IsAvailable);
query = query.OrderBy(p => p.Name);

// Step 3 — THIS is where the SQL fires and the DB is hit
List<Product> results = query.ToList();  // ← execution trigger
```

The first three lines produce no SQL at all. EF Core is accumulating the conditions into an **expression tree** — a data structure that represents the query. Only `.ToList()` translates it into SQL and executes it.

---

## `IQueryable<T>` vs `IEnumerable<T>` — The Key Distinction

This is the most important thing to understand about deferred execution.

`IQueryable<T>` — query lives in the **database**. Filters and operations happen in SQL before data comes to your app.

`IEnumerable<T>` — query lives in **memory**. Data is already fetched; operations happen in C# on what's already loaded.

```csharp
// IQueryable — filter happens in SQL (WHERE clause)
IQueryable<Product> query = context.Products
    .Where(p => p.Price > 50);   // translated to SQL

// IEnumerable — ALL products loaded into memory first, THEN filtered in C#
IEnumerable<Product> inMemory = context.Products.AsEnumerable();
var filtered = inMemory.Where(p => p.Price > 50);  // C# loop, not SQL
```

If you have 1 million products and only 10 match the filter — `IQueryable` fetches 10 rows. `IEnumerable` fetches all 1 million into memory and then filters. The difference is enormous.

---

## Execution Triggers — What Forces the Query to Run

These methods materialise the query and hit the database:

```csharp
.ToList()           // → List<T>
.ToArray()          // → T[]
.ToDictionary()     // → Dictionary<K,V>
.First()            // → single item, throws if empty
.FirstOrDefault()   // → single item or null
.Single()           // → single item, throws if 0 or >1
.SingleOrDefault()  // → single item or null, throws if >1
.Count()            // → int
.Any()              // → bool
.Sum() / .Min() / .Max() / .Average()
.ToListAsync()      // → async versions of all the above
foreach (var x in query) { }  // ← iteration also triggers execution
```

Every one of these says "I need the data now." EF Core translates the accumulated expression tree into SQL and executes it.

---

## Why Deferred Execution Is Powerful

### Build Queries Conditionally — No String Concatenation

```csharp
var query = context.Products.AsQueryable();

// Conditionally add filters based on user input
if (minPrice.HasValue)
    query = query.Where(p => p.Price >= minPrice.Value);

if (categoryId.HasValue)
    query = query.Where(p => p.CategoryId == categoryId.Value);

if (!string.IsNullOrEmpty(searchTerm))
    query = query.Where(p => p.Name.Contains(searchTerm));

// One SQL query, only with the relevant WHERE clauses
var results = query.ToList();
```

Without deferred execution you'd either need multiple queries or messy SQL string building. Here you compose the query step by step and it executes once — cleanly.

---

### Pagination — Compose First, Execute Once

```csharp
var query = context.Orders
    .Where(o => o.CustomerId == customerId)
    .OrderByDescending(o => o.Date);

int totalCount = query.Count();           // first execution — COUNT query

var page = query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToList();                            // second execution — paginated SELECT
```

Each execution trigger produces its own SQL. The `Count()` generates `SELECT COUNT(*)`. The `ToList()` generates `SELECT ... LIMIT x OFFSET y`. The original query object is reusable.

---

## The Deferred Execution Trap — Accidental Multiple Execution

Because the query re-executes every time you trigger it, be careful:

```csharp
var query = context.Products.Where(p => p.Price > 50);

var count  = query.Count();   // DB hit #1
var first  = query.First();   // DB hit #2
var list   = query.ToList();  // DB hit #3
```

Three separate SQL queries ran against the database. If you need all three, materialise once:

```csharp
var products = context.Products
    .Where(p => p.Price > 50)
    .ToList();                    // ← single DB hit

var count = products.Count;       // in-memory, no DB
var first = products.First();     // in-memory, no DB
```

---

## The Broken Closure Trap — Variable Captured by Reference

```csharp
int minPrice = 50;

var query = context.Products
    .Where(p => p.Price > minPrice);  // minPrice captured by reference

minPrice = 100;  // ← changed BEFORE execution

var results = query.ToList();  // executes with minPrice = 100, not 50!
```

Because execution is deferred, the variable is read at execution time — not at query-definition time. In loops this is a classic bug:

```csharp
var queries = new List<IQueryable<Product>>();

for (int i = 0; i < 5; i++)
{
    queries.Add(context.Products.Where(p => p.CategoryId == i));
}

// All 5 queries execute with i = 5 (the final loop value)
foreach (var q in queries)
    q.ToList();
```

Fix — capture the loop variable into a local:

```csharp
for (int i = 0; i < 5; i++)
{
    int capturedI = i;  // ← local copy, safe from mutation
    queries.Add(context.Products.Where(p => p.CategoryId == capturedI));
}
```

---

## Where Deferred Execution Breaks Down — Client Evaluation

Sometimes EF Core can't translate your C# into SQL. It either throws or (in older versions) silently evaluates in memory:

```csharp
// This works — EF can translate Contains to SQL LIKE
var results = context.Products
    .Where(p => p.Name.Contains("bike"))
    .ToList();

// This fails — custom C# method can't be translated to SQL
var results = context.Products
    .Where(p => MyCustomFilter(p))   // ← EF doesn't know what this is
    .ToList();
// → throws InvalidOperationException in EF Core 3+
```

When EF Core can't translate something, it tells you loudly (in EF Core 3+). The fix is to either restructure the query using translatable expressions, or materialise first and then apply the custom logic in memory:

```csharp
var results = context.Products
    .Where(p => p.Price > 50)      // ← translated, runs in SQL
    .ToList()                       // ← materialise here
    .Where(p => MyCustomFilter(p)); // ← custom logic runs in C# on the result
```

---

## Summary

```
IQueryable    = a recipe, not the meal — SQL not sent yet
IEnumerable   = the meal is already cooked — data is in memory

Deferred      = query builds up, SQL fires only at the trigger
Triggers      = ToList(), First(), Count(), Any(), foreach, etc.

Power         = compose queries conditionally, reuse, paginate cleanly
Traps         = multiple triggers   → multiple DB hits
              = captured variables  → read at execution time, not definition time
              = untranslatable C#   → InvalidOperationException
```
