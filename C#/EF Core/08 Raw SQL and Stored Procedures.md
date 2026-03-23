# Raw SQL, Stored Procedures & When to Step Outside LINQ

---

## Why Step Outside LINQ at All?

LINQ and EF Core's query translator are powerful, but they have a ceiling. There are things SQL does natively that LINQ either can't express, translates poorly, or produces suboptimal SQL for. When that happens, EF Core lets you drop down to raw SQL — without losing change tracking or entity materialisation.

---

## `FromSqlRaw()` — Raw SQL That Returns Entities

The most common escape hatch. Runs a raw SQL query and materialises the result into tracked entity objects — just like a normal LINQ query.

```csharp
var products = context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE Price > 50")
    .ToList();
```

The returned `Product` objects are fully tracked — you can modify them and call `SaveChanges()`.

### Composing LINQ on Top of Raw SQL

The real power: you can chain LINQ operators on top of the raw SQL. EF Core wraps your SQL in a subquery and applies the LINQ as an outer query.

```csharp
var results = context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE IsDiscontinued = 0")
    .Where(p => p.Price < 100)         // ← LINQ on top of raw SQL
    .OrderBy(p => p.Name)
    .Take(20)
    .ToList();
```

Generated SQL:
```sql
SELECT TOP 20 p.*
FROM (
    SELECT * FROM Products WHERE IsDiscontinued = 0
) AS p
WHERE p.Price < 100
ORDER BY p.Name
```

---

## `FromSqlInterpolated()` — Safe Parameterisation

Never concatenate user input directly into SQL strings. Use `FromSqlInterpolated()` which converts the interpolated values into proper SQL parameters automatically:

```csharp
decimal minPrice = 50m;
string  category = "Electronics";

// ✅ Safe — values become @p0, @p1 parameters
var products = context.Products
    .FromSqlInterpolated(
        $"SELECT * FROM Products WHERE Price > {minPrice} AND Category = {category}")
    .ToList();

// ❌ SQL injection risk — never do this
var products = context.Products
    .FromSqlRaw($"SELECT * FROM Products WHERE Category = '{category}'")
    .ToList();
```

`FromSqlInterpolated` looks like string interpolation but isn't — it uses `FormattableString` to extract the values and bind them as parameters, not string-concatenated into the SQL.

---

## `SqlQuery<T>()` — Raw SQL for Non-Entity Types

`FromSqlRaw` requires the result to map to a `DbSet<T>` entity. For arbitrary projections — scalar values, DTOs, aggregates — use `SqlQuery<T>()` (EF Core 7+):

```csharp
// Returns a non-entity type — no DbSet needed
var summaries = context.Database
    .SqlQuery<OrderSummary>(
        $"SELECT o.Id, c.Name AS CustomerName, o.Total FROM Orders o JOIN Customers c ON c.Id = o.CustomerId")
    .Where(s => s.Total > 100)  // ← LINQ composable
    .ToList();
```

For older EF Core versions, the equivalent was using `FromSqlRaw` with a keyless entity (a model class configured with `.HasNoKey()`).

---

## `ExecuteSqlRaw()` — Commands That Don't Return Entities

For `UPDATE`, `DELETE`, `INSERT`, or any statement that doesn't return rows:

```csharp
// Bulk update — far faster than loading entities and saving one by one
int rowsAffected = context.Database
    .ExecuteSqlRaw(
        "UPDATE Products SET Price = Price * 0.9 WHERE CategoryId = {0}", categoryId);

// With interpolation — safe parameterisation
context.Database
    .ExecuteSqlInterpolated(
        $"DELETE FROM AuditLogs WHERE CreatedAt < {DateTime.UtcNow.AddDays(-90)}");
```

This bypasses EF Core's change tracker entirely — nothing gets loaded into memory. It's the right tool for bulk operations where loading thousands of entities would be wasteful.

---

## Stored Procedures — `FromSqlRaw` + `EXEC`

Call stored procedures exactly as you'd write SQL:

```csharp
// Stored procedure returning rows — maps to entities
var products = context.Products
    .FromSqlRaw("EXEC GetProductsByCategory @CategoryId = {0}", categoryId)
    .ToList();

// With multiple parameters
var orders = context.Orders
    .FromSqlRaw("EXEC GetOrdersInRange @From = {0}, @To = {1}",
        DateTime.UtcNow.AddDays(-30), DateTime.UtcNow)
    .ToList();
```

### Stored Procedures With Output Parameters

For stored procedures that return output parameters alongside rows, you need `SqlParameter` directly:

```csharp
var totalParam = new SqlParameter("@Total", SqlDbType.Decimal)
{
    Direction = ParameterDirection.Output
};

context.Database.ExecuteSqlRaw(
    "EXEC GetOrderSummary @CustomerId = {0}, @Total = @Total OUTPUT",
    customerId, totalParam);

decimal total = (decimal)totalParam.Value;
```

### Stored Procedures That Don't Return Rows

```csharp
context.Database.ExecuteSqlRaw(
    "EXEC ArchiveOldOrders @CutoffDate = {0}", DateTime.UtcNow.AddYears(-1));
```

---

## Keyless Entities — DTOs Without Primary Keys

When you have a stored procedure or view that returns data not matching any entity, define a **keyless entity** — a model class EF Core can materialise into but never tracks or inserts:

```csharp
public class SalesSummary
{
    public string Category { get; set; }
    public int TotalOrders { get; set; }
    public decimal Revenue { get; set; }
}
```

```csharp
// Configure as keyless — no primary key, no tracking
modelBuilder.Entity<SalesSummary>().HasNoKey();
```

```csharp
var summary = context.Set<SalesSummary>()
    .FromSqlRaw("EXEC GetSalesSummary")
    .ToList();
```

Keyless entities can also map to **database views**:

```csharp
modelBuilder.Entity<SalesSummary>()
    .HasNoKey()
    .ToView("vw_SalesSummary");  // ← maps to a view, not a table

// Now queryable like a normal DbSet
var summary = context.Set<SalesSummary>().ToList();
```

---

## When to Step Outside LINQ

LINQ is the right default. Step outside it when:

**Performance-critical bulk operations** — loading 50,000 entities to update one column is wasteful. One `ExecuteSqlRaw` UPDATE does it in milliseconds.

```csharp
// ❌ Load all, modify all, save all — 50,000 round-trips
var products = context.Products.Where(p => p.CategoryId == 5).ToList();
foreach (var p in products) p.IsDiscontinued = true;
context.SaveChanges();

// ✅ One SQL statement
context.Database.ExecuteSqlRaw(
    "UPDATE Products SET IsDiscontinued = 1 WHERE CategoryId = {0}", 5);
```

**SQL features LINQ can't express** — window functions (`ROW_NUMBER`, `RANK`, `LAG`), CTEs, `MERGE`, recursive queries:

```csharp
// Window function — no LINQ equivalent
var ranked = context.Database
    .SqlQuery<ProductRank>(
        $"SELECT Id, Name, Price, ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY Price DESC) AS Rank FROM Products")
    .ToList();
```

**Stored procedures required by the DBA or organisation** — many enterprises mandate stored procedures for all data access. EF Core accommodates this without abandoning the rest of the ORM.

**Complex aggregations and reporting queries** — deeply nested CTEs and multi-level aggregations often produce cleaner, faster SQL when written by hand than what EF Core generates from LINQ.

---

## What Raw SQL Loses

When you use raw SQL, be aware of what EF Core can no longer do automatically:

```csharp
// Change tracking — still works with FromSqlRaw on a DbSet
var product = context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE Id = {0}", id)
    .Single();
product.Price = 99m;
context.SaveChanges();  // ✅ still tracked, UPDATE generated

// BUT — ExecuteSqlRaw bypasses change tracking entirely
context.Database.ExecuteSqlRaw("UPDATE Products SET Price = 99 WHERE Id = {0}", id);
// The in-memory entity (if loaded) is now stale — context doesn't know it changed
```

If you modify data with raw SQL while entities are loaded in the same context, reload them afterward or clear the change tracker:

```csharp
context.Database.ExecuteSqlRaw("UPDATE Products SET Price = 99 WHERE Id = {0}", id);
context.ChangeTracker.Clear();  // ← discard stale tracked entities
```

---

## Summary

```
FromSqlRaw()             → raw SELECT mapped to entities, fully tracked
                         → LINQ composable on top
FromSqlInterpolated()    → same, but interpolated values become safe parameters
SqlQuery<T>()            → raw SELECT mapped to any type, no DbSet needed (EF7+)
ExecuteSqlRaw()          → UPDATE/DELETE/INSERT, bypasses change tracker
Stored procedures        → called via FromSqlRaw("EXEC ...") or ExecuteSqlRaw
Keyless entities         → DTOs or views — materialisable but never tracked

When to use raw SQL:
  → bulk operations (UPDATE/DELETE without loading entities)
  → SQL features LINQ can't express (window functions, CTEs, MERGE)
  → stored procedures mandated by the organisation
  → complex aggregation and reporting queries

What to watch out for:
  → always use parameterised overloads — never concatenate user input
  → ExecuteSqlRaw bypasses change tracker — reload or clear after bulk ops
  → FromSqlRaw still requires SELECT * or all mapped columns for full materialisation
```
