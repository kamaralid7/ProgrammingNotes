# Compiled Queries & Query Plan Caching

---

## The Hidden Cost of Every Query

Every time you run a LINQ query in EF Core, before any SQL hits the database, EF Core does invisible work:

```
Your LINQ query
      ↓
Parse the expression tree
      ↓
Translate to SQL
      ↓
Compile the SQL string
      ↓
Execute against the database
```

Steps 1–3 happen entirely in C# on the CPU — pure overhead before any database work begins. For a query you run thousands of times a second, this adds up.

---

## EF Core's Automatic Query Cache

EF Core already caches compiled query plans automatically. The first time a query runs, the expression tree is translated and the result stored in an internal cache keyed by the query's structure. Subsequent calls with the same shape hit the cache and skip translation entirely.

```csharp
// First call — full translation cost paid
var result1 = context.Products.Where(p => p.Price > 50).ToList();

// Second call — same structure, cache hit, translation skipped
var result2 = context.Products.Where(p => p.Price > 50).ToList();
```

This works well for most queries. But the cache key is based on the **structure** of the expression tree — if anything changes the structure, it's a cache miss.

---

## What Breaks the Cache

```csharp
// ✅ Cache hit — structure identical, only the value differs (parameterised)
var a = context.Products.Where(p => p.Price > 50).ToList();
var b = context.Products.Where(p => p.Price > 99).ToList();

// ❌ Cache miss — structure changed by conditional composition
IQueryable<Product> query = context.Products;
if (filterByCategory)
    query = query.Where(p => p.CategoryId == 1);  // different tree shape each time
var c = query.ToList();
```

Every unique tree shape is translated and cached separately. If you dynamically compose queries in many permutations, you can flood the cache — each permutation occupies a cache slot.

---

## `EF.CompileQuery()` — Explicit Compiled Queries

For hot paths — queries that run thousands of times — you can pay the translation cost exactly once at startup and reuse the compiled result forever.

```csharp
// Compiled once — at startup or as a static field
private static readonly Func<ShopContext, decimal, List<Product>> _getAffordableProducts
    = EF.CompileQuery((ShopContext ctx, decimal maxPrice) =>
        ctx.Products
           .Where(p => p.Price < maxPrice && !p.IsDiscontinued)
           .OrderBy(p => p.Name)
           .ToList());
```

```csharp
// Used many times — zero translation overhead each call
var products = _getAffordableProducts(context, 100m);
var budget   = _getAffordableProducts(context, 50m);
```

The `EF.CompileQuery()` call runs once — translates the expression tree, produces a query plan, and returns a plain `Func`. Every subsequent call invokes that `Func` directly — no expression tree, no translation, no cache lookup. Just SQL parameter binding and execution.

---

## Async Version

```csharp
private static readonly Func<ShopContext, decimal, IAsyncEnumerable<Product>> _getAffordableAsync
    = EF.CompileAsyncQuery((ShopContext ctx, decimal maxPrice) =>
        ctx.Products
           .Where(p => p.Price < maxPrice && !p.IsDiscontinued)
           .OrderBy(p => p.Name));

// Usage
await foreach (var product in _getAffordableAsync(context, 100m))
{
    Console.WriteLine(product.Name);
}
```

---

## Where to Put Compiled Queries — Static Fields on the Repository

```csharp
public class ProductRepository
{
    private readonly ShopContext _context;

    private static readonly Func<ShopContext, int, Product?> _getById
        = EF.CompileQuery((ShopContext ctx, int id) =>
            ctx.Products
               .Include(p => p.Category)
               .FirstOrDefault(p => p.Id == id));

    private static readonly Func<ShopContext, decimal, IEnumerable<Product>> _getBelowPrice
        = EF.CompileQuery((ShopContext ctx, decimal max) =>
            ctx.Products
               .Where(p => p.Price < max && !p.IsDiscontinued)
               .OrderBy(p => p.Price));

    public ProductRepository(ShopContext context) => _context = context;

    public Product? GetById(int id)          => _getById(_context, id);
    public IEnumerable<Product> GetBelowPrice(decimal max) => _getBelowPrice(_context, max);
}
```

---

## Limitations of Compiled Queries

```csharp
// ✅ Can parameterise values — prices, ids, strings, dates, bool flags
EF.CompileQuery((ShopContext ctx, decimal max, bool includeDiscontinued) =>
    ctx.Products
       .Where(p => p.Price < max && (includeDiscontinued || !p.IsDiscontinued))
       .ToList());

// ❌ Cannot compose dynamically — shape must be fixed at compile time
// ❌ Cannot Include() dynamically — navigation paths must be fixed
// ❌ Cannot change OrderBy direction dynamically
```

For fully dynamic queries (user-defined filters, dynamic sort columns) stick with the automatic cache. Compiled queries are for **known, fixed-shape, high-frequency** queries.

---

## The Performance Picture

```
Regular query (first run):   Parse tree → translate → cache → execute
Regular query (cached):      Cache lookup → execute
Compiled query:              Execute  ← skip everything else
```

In benchmarks, compiled queries are typically **2–5× faster** for simple queries because the entire translation pipeline is eliminated. For complex queries with many joins, the gap is even larger.

---

## Summary

```
Automatic cache:    EF Core caches query plans by expression tree shape
                    → same shape = cache hit, translation skipped
                    → dynamic composition = new shapes = cache misses

EF.CompileQuery():  Pay translation cost once at startup
                    → returns a plain Func — no tree, no translation, no cache lookup
                    → best for hot paths: high-frequency, fixed-shape queries
                    → store as static fields on repository classes

Limitations:        Fixed shape only — no dynamic includes or sort directions
                    Parameters OK — values, flags, ids, strings, dates
```
