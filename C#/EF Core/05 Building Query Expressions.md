# Building Query Expressions — Delegates vs Expression Trees

---

## The Two Parallel Worlds of LINQ

LINQ has two completely separate sets of operators living side by side — and the same lambda syntax feeds into both, but produces entirely different things depending on which world you're in.

**Local queries** use `Enumerable` operators and take **delegates**.
**Interpreted queries** use `Queryable` operators and take **expression trees**.

---

## Comparing the Signatures Directly

```csharp
// Enumerable.Where — accepts a DELEGATE
public static IEnumerable<T> Where<T>(
    this IEnumerable<T> source,
    Func<T, bool> predicate)              // ← compiled code

// Queryable.Where — accepts an EXPRESSION TREE
public static IQueryable<T> Where<T>(
    this IQueryable<T> source,
    Expression<Func<T, bool>> predicate)  // ← data structure
```

Same method name. Same lambda syntax at the call site. Completely different parameter types. The compiler decides which one to call based on whether the source is `IEnumerable<T>` or `IQueryable<T>`.

---

## Same Lambda, Two Different Compilations

```csharp
// These lambdas look IDENTICAL
p => p.Price > 50

// But feed into different overloads depending on the source type

IEnumerable<Product> localList = GetProductsFromMemory();
IQueryable<Product>  dbQuery   = context.Products;

// Compiler resolves to Enumerable.Where → lambda becomes a Func (delegate)
var localResult = localList.Where(p => p.Price > 50);

// Compiler resolves to Queryable.Where → lambda becomes Expression<Func> (tree)
var dbResult    = dbQuery.Where(p => p.Price > 50);
```

The lambda `p => p.Price > 50` is written once but the compiler emits two entirely different things:

- For `IEnumerable` → **compiled IL bytecode**, wrapped in a `Func<Product, bool>`
- For `IQueryable` → **expression tree object**, an `Expression<Func<Product, bool>>`

---

## What Each One Actually Is

### Delegate — Compiled Code, Ready to Run

A `Func<T, bool>` is a **delegate** — a compiled piece of executable code living in memory. When you invoke it, it runs. You can call it. You cannot look inside it.

```csharp
Func<Product, bool> isExpensive = p => p.Price > 50;

// It's just a function — call it directly
bool result = isExpensive(new Product { Price = 99 });  // → true
```

The compiler turns the lambda into IL (Intermediate Language). The structure is gone — it's a sealed envelope.

### Expression Tree — Code as Data

An `Expression<Func<T, bool>>` is **not** compiled code. It's a **description** of the code — a tree structure in memory you can inspect, traverse, and translate.

```csharp
Expression<Func<Product, bool>> expr = p => p.Price > 50;

Console.WriteLine(expr.Body);          // → (p.Price > 50)
Console.WriteLine(expr.Body.NodeType); // → GreaterThan
```

The compiler builds a tree of objects instead of IL:

```
BinaryExpression (GreaterThan)
├── Left:  MemberExpression (p.Price)
│           └── ParameterExpression (p)
└── Right: ConstantExpression (50)
```

EF Core walks this tree and translates each node into SQL — `GreaterThan` → `>`, `MemberExpression` → column name, `ConstantExpression` → value.

---

## Predicate1 and Predicate2 Are NOT Interchangeable

```csharp
Func<Product, bool>             predicate1 = p => p.Price > 50;  // delegate
Expression<Func<Product, bool>> predicate2 = p => p.Price > 50;  // expression tree
```

Even though both hold the same logical condition, you **cannot** swap them:

```csharp
// ✅ Works — Enumerable.Where accepts Func
localList.Where(predicate1);

// ✅ Works — Queryable.Where accepts Expression<Func>
dbQuery.Where(predicate2);

// ❌ Compile error — cannot pass Func to Queryable.Where
dbQuery.Where(predicate1);
// → Argument type 'Func<Product, bool>' is not assignable to
//   parameter type 'Expression<Func<Product, bool>>'

// ❌ Compile error — cannot pass Expression<Func> to Enumerable.Where
localList.Where(predicate2);
// → Argument type 'Expression<Func<Product, bool>>' is not assignable to
//   parameter type 'Func<Product, bool>'
```

The type system enforces this boundary hard. No implicit conversion exists between the two.

---

## What Happens If You Force the Wrong One

If you accidentally pull a `IQueryable` into `IEnumerable` territory before filtering:

```csharp
Func<Product, bool> predicate1 = p => p.Price > 50;

// AsEnumerable() switches from IQueryable to IEnumerable
var results = context.Products
    .AsEnumerable()         // ← ALL products loaded into memory NOW
    .Where(predicate1);     // ← filter runs in C#, not SQL
```

The predicate is a delegate — opaque compiled code. EF Core can't read it, can't translate it. It has no choice but to load everything first and let C# do the filtering. You've pulled your entire `Products` table into RAM.

---

## How EF Core Uses Expression Trees

When you write:

```csharp
context.Products.Where(p => p.Price > 50)
```

`Where()` on `IQueryable<T>` receives an expression tree. EF Core's query translator walks it:

```
BinaryExpression (GreaterThan)
├── MemberExpression → column name "Price"
└── ConstantExpression → value 50

→ Translates to: WHERE Price > 50
```

If it were a plain `Func`, EF Core would have compiled bytecode — no way to read it, no way to generate SQL. That's exactly what `AsEnumerable()` forces.

---

## The One-Way Bridge — Compile an Expression Into a Delegate

You can go from expression tree → delegate, but **not** the other way:

```csharp
Expression<Func<Product, bool>> predicate2 = p => p.Price > 50;

// Expression → Delegate: possible via .Compile()
Func<Product, bool> predicate1 = predicate2.Compile();

// Delegate → Expression: IMPOSSIBLE
// There's no way to decompile bytecode back into an inspectable tree
Expression<Func<Product, bool>> impossible = predicate1; // ← compile error
```

Once code is compiled to IL, the structure is gone — you can't reverse-engineer a `Func` back into an expression tree.

Storing predicates as `Expression<Func<T, bool>>` gives you flexibility — use them in both worlds:

```csharp
Expression<Func<Product, bool>> isAvailableAndCheap =
    p => p.IsAvailable && p.Price < 100;

// Use with EF Core — translates to SQL
var dbProducts = context.Products
    .Where(isAvailableAndCheap)
    .ToList();
// → SELECT * FROM Products WHERE IsAvailable = 1 AND Price < 100

// Use in memory — compile first
var compiled = isAvailableAndCheap.Compile();
var inMemoryProducts = cachedProducts.Where(compiled);
```

---

## Building Expression Trees Manually

Usually the compiler builds expression trees from lambdas. But you can build them programmatically — useful when the query shape isn't known until runtime:

```csharp
// Manually build: p => p.Price > 50

var parameter = Expression.Parameter(typeof(Product), "p");       // p
var property  = Expression.Property(parameter, nameof(Product.Price)); // p.Price
var constant  = Expression.Constant(50m, typeof(decimal));        // 50
var body      = Expression.GreaterThan(property, constant);       // p.Price > 50
var lambda    = Expression.Lambda<Func<Product, bool>>(body, parameter);

// Use it exactly like a hand-written lambda
var results = context.Products.Where(lambda).ToList();
// → SELECT * FROM Products WHERE Price > 50
```

### Dynamic Filter Builder — Real-World Use

```csharp
public IQueryable<Product> BuildQuery(SearchRequest request)
{
    var parameter  = Expression.Parameter(typeof(Product), "p");
    var conditions = new List<Expression>();

    if (request.MinPrice.HasValue)
    {
        var prop  = Expression.Property(parameter, nameof(Product.Price));
        var value = Expression.Constant(request.MinPrice.Value);
        conditions.Add(Expression.GreaterThanOrEqual(prop, value));
    }

    if (!string.IsNullOrEmpty(request.Name))
    {
        var prop     = Expression.Property(parameter, nameof(Product.Name));
        var value    = Expression.Constant(request.Name);
        var contains = Expression.Call(prop,
                           typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                           value);
        conditions.Add(contains);
    }

    if (conditions.Count == 0)
        return context.Products;

    // Combine all conditions with AND
    var body   = conditions.Aggregate(Expression.AndAlso);
    var lambda = Expression.Lambda<Func<Product, bool>>(body, parameter);

    return context.Products.Where(lambda);
}
```

Every filter the user picks gets composed into one expression tree → one SQL query with only the relevant `WHERE` clauses. No conditional string building. No multiple queries.

---

## Summary

```
Local queries      → IEnumerable → Enumerable.Where → takes Func (delegate)
                   → runs in C# memory, data already loaded

Interpreted queries → IQueryable → Queryable.Where → takes Expression<Func> (tree)
                    → translated to SQL by EF Core, runs in database

Same lambda syntax → compiler picks the right overload based on source type

Func and Expression<Func>:
  → same logic, different types, NOT interchangeable
  → Expression → Func: yes, via .Compile()
  → Func → Expression: impossible, bytecode cannot be decompiled

Store predicates as Expression<Func<T,bool>> for maximum flexibility —
they work in both the database world and the in-memory world.
```

---

# Compiling Expression Trees — One Predicate, Both Worlds

---

## The Pattern — Static Predicate on the Entity Itself

A clean pattern is to define the predicate **on the entity class it describes**. The entity knows its own rules best — having it live there means you write the logic once and reuse it everywhere.

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDiscontinued { get; set; }
    public DateTime LastSoldOn { get; set; }
    public decimal Price { get; set; }

    // Returns an Expression — works with IQueryable (EF Core → SQL)
    public static Expression<Func<Product, bool>> IsSelling()
    {
        return p => !p.IsDiscontinued
                 && p.LastSoldOn >= DateTime.UtcNow.AddDays(-30);
    }
}
```

The method returns `Expression<Func<Product, bool>>` — a tree, not compiled code. EF Core can translate it to SQL. And because it's an expression, you can also `.Compile()` it into a delegate when you need to run it in memory.

---

## Using It in an Interpreted Query — SQL Path

```csharp
// IQueryable → Queryable.Where → accepts Expression<Func<T,bool>>
var sellingProducts = context.Products
    .Where(Product.IsSelling())
    .ToList();
```

Generated SQL:
```sql
SELECT *
FROM Products
WHERE IsDiscontinued = 0
  AND LastSoldOn >= '2026-02-21'   -- 30 days back from today
```

EF Core walks the expression tree, reads each node, and produces the `WHERE` clause. No data leaves the database until after filtering.

---

## Using It in a Local Query — In-Memory Path

```csharp
// .Compile() turns the expression tree into a callable delegate
Func<Product, bool> isSellingDelegate = Product.IsSelling().Compile();

// IEnumerable → Enumerable.Where → accepts Func<T,bool>
List<Product> cachedProducts = GetFromCache();

var sellingProducts = cachedProducts
    .Where(isSellingDelegate)
    .ToList();
```

Same predicate logic. Same result. Running in C# memory against a local list — no database involved.

---

## Both Forms From One Definition

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDiscontinued { get; set; }
    public DateTime LastSoldOn { get; set; }

    // The expression — for IQueryable / EF Core
    public static Expression<Func<Product, bool>> IsSelling()
        => p => !p.IsDiscontinued
             && p.LastSoldOn >= DateTime.UtcNow.AddDays(-30);

    // The compiled delegate — for IEnumerable / in-memory
    public static Func<Product, bool> IsSellingFunc()
        => IsSelling().Compile();
}
```

```csharp
// Interpreted query — hits the database
var fromDb = context.Products
    .Where(Product.IsSelling())
    .ToList();

// Local query — runs in memory
var fromCache = cachedProducts
    .Where(Product.IsSellingFunc())
    .ToList();
```

Both paths use exactly the same condition, defined exactly once.

---

## Why This Is Better Than Two Separate Predicates

```csharp
// ❌ Fragile — two definitions of the same rule, will drift apart
context.Products.Where(p => !p.IsDiscontinued && p.LastSoldOn >= DateTime.UtcNow.AddDays(-30));
cachedProducts.Where(p => !p.IsDiscontinued && p.LastSoldOn >= DateTime.UtcNow.AddDays(-30));
// Someone updates one, forgets the other → inconsistent behaviour

// ✅ Single source of truth
context.Products.Where(Product.IsSelling());
cachedProducts.Where(Product.IsSellingFunc());
// Change it once → both paths updated automatically
```

---

## Composing With Other Conditions

```csharp
var results = context.Products
    .Where(Product.IsSelling())       // ← reusable rule
    .Where(p => p.Price < 100)        // ← additional condition
    .OrderBy(p => p.Name)
    .Take(20)
    .ToList();
```

Generated SQL:
```sql
SELECT TOP 20 *
FROM Products
WHERE IsDiscontinued = 0
  AND LastSoldOn >= '2026-02-21'
  AND Price < 100
ORDER BY Name
```

One round-trip. Everything pushed to the database.

---

## Summary

```
.Compile()         → converts Expression<Func<T,bool>> into Func<T,bool>
                   → expression tree becomes executable delegate

Static predicate on entity   → single source of truth for business rules
    used with IQueryable      → EF Core translates to SQL WHERE clause
    used with .Compile()      → runs as C# filter over in-memory list

Same logic. One definition. Both worlds.
```

---

# PredicateBuilder — Combining Expressions with AND / OR at Runtime

---

## The Problem — Expression Trees Can't Be Combined Directly

You have two separate expressions. You want to combine them:

```csharp
Expression<Func<Product, bool>> isSelling = p => !p.IsDiscontinued
                                               && p.LastSoldOn >= DateTime.UtcNow.AddDays(-30);
Expression<Func<Product, bool>> isCheap   = p => p.Price < 100;

// ❌ This does NOT work — you can't use && on expression trees
Expression<Func<Product, bool>> combined  = p => isSelling && isCheap;
```

Each expression has its own parameter `p`. They're separate objects. The compiler has no idea how to merge two expression trees with `&&`.

---

## The Parameter Problem — Why Naive Combining Fails

```csharp
// Each lambda has its OWN parameter instance
Expression<Func<Product, bool>> expr1 = p => p.Price > 50;    // parameter "p" instance A
Expression<Func<Product, bool>> expr2 = p => p.Price < 200;   // parameter "p" instance B
```

Even though both are named `p`, they're two different `ParameterExpression` objects. If you just smash the bodies together:

```csharp
// Naive attempt
var combined = Expression.AndAlso(expr1.Body, expr2.Body);
// ❌ The two bodies reference DIFFERENT parameter objects
// EF Core sees two unrelated parameters — blows up at translation
```

To combine them correctly you need to **replace** one parameter with the other throughout its tree — this is what `ExpressionVisitor` does.

---

## The Solution — Parameter Replacement Visitor

```csharp
public class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _from;
    private readonly ParameterExpression _to;

    public ParameterReplacer(ParameterExpression from, ParameterExpression to)
    {
        _from = from;
        _to   = to;
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _from ? _to : base.VisitParameter(node);
}
```

Now combine two expressions safely:

```csharp
public static Expression<Func<T, bool>> And<T>(
    this Expression<Func<T, bool>> left,
    Expression<Func<T, bool>> right)
{
    // Rewrite right's body so it uses left's parameter
    var rewriter  = new ParameterReplacer(right.Parameters[0], left.Parameters[0]);
    var rightBody = rewriter.Visit(right.Body);

    return Expression.Lambda<Func<T, bool>>(
        Expression.AndAlso(left.Body, rightBody),
        left.Parameters[0]);
}

public static Expression<Func<T, bool>> Or<T>(
    this Expression<Func<T, bool>> left,
    Expression<Func<T, bool>> right)
{
    var rewriter  = new ParameterReplacer(right.Parameters[0], left.Parameters[0]);
    var rightBody = rewriter.Visit(right.Body);

    return Expression.Lambda<Func<T, bool>>(
        Expression.OrElse(left.Body, rightBody),
        left.Parameters[0]);
}
```

---

## Using It — Clean and Readable

```csharp
Expression<Func<Product, bool>> isSelling  = Product.IsSelling();
Expression<Func<Product, bool>> isCheap    = p => p.Price < 100;
Expression<Func<Product, bool>> isFeatured = p => p.IsFeatured;

// Combine with AND
var andCombined = isSelling.And(isCheap);

// Combine with OR
var orCombined  = isSelling.Or(isFeatured);

// Chain as many as you like
var complex = isSelling.And(isCheap).Or(isFeatured);

// Translates directly to SQL
var results = context.Products.Where(andCombined).ToList();
```

Generated SQL for `isSelling.And(isCheap)`:
```sql
SELECT * FROM Products
WHERE IsDiscontinued = 0
  AND LastSoldOn >= '2026-02-21'
  AND Price < 100
```

---

## Real-World Use — Role-Based Filter Builder

```csharp
public Expression<Func<Product, bool>> BuildAccessFilter(UserRole role)
{
    var filter = Product.IsSelling();  // everyone sees active products

    if (role == UserRole.Admin)
        filter = filter.Or(p => p.IsDiscontinued);   // admins also see discontinued

    if (role == UserRole.Premium)
        filter = filter.And(p => p.Price <= 10_000); // premium capped at price

    return filter;
}

var filter   = BuildAccessFilter(currentUser.Role);
var products = context.Products.Where(filter).ToList();
```

One method. One SQL query. No branching code paths scattered across the codebase.

---

## LINQKit — PredicateBuilder Ready-Made

Rather than writing `ParameterReplacer` yourself, **LINQKit** ships a battle-tested `PredicateBuilder`:

```csharp
// Install: dotnet add package LINQKit
using LinqKit;

// false = OR base — nothing matches until you add inclusions
var predicate = PredicateBuilder.New<Product>(false);

predicate = predicate.Or(p => p.CategoryId == 1);
predicate = predicate.Or(p => p.CategoryId == 2);
predicate = predicate.Or(p => p.CategoryId == 3);

// → WHERE CategoryId = 1 OR CategoryId = 2 OR CategoryId = 3
var results = context.Products
    .AsExpandable()   // ← LINQKit hook that expands the predicate
    .Where(predicate)
    .ToList();
```

`PredicateBuilder.New<T>(true)` starts with AND — everything matches until you restrict.
`PredicateBuilder.New<T>(false)` starts with OR — nothing matches until you include.

---

## Summary

```
Problem:    Expression trees have separate parameter instances — can't just use &&
Solution:   ExpressionVisitor rewrites one tree to share the other's parameter

.And(expr)  → Expression.AndAlso → SQL AND
.Or(expr)   → Expression.OrElse  → SQL OR

Use cases:
  → Role-based filters built at runtime
  → Search pages with any combination of user-selected criteria
  → Reusing entity predicates and combining them per feature

LINQKit PredicateBuilder → ready-made, production-tested implementation
```

---

# `AsQueryable()` — One Query, Local or Remote

---

## The Problem It Solves

You want to write a filtering method once and have it work whether the data comes from a database or an in-memory list. Without `AsQueryable()` you'd need two versions of every method: one taking `IQueryable<T>` for EF Core, one taking `IEnumerable<T>` for local lists. `AsQueryable()` bridges that gap.

---

## What It Does

When called on an `IEnumerable<T>`, `AsQueryable()` wraps it in an `IQueryable<T>` that uses **LINQ to Objects** under the hood — meaning the query operators still accept `Expression<Func<T,bool>>` signatures, but execution compiles and runs them in memory.

```csharp
List<Product> localList = GetFromCache();

// Wrap the list as IQueryable
IQueryable<Product> queryable = localList.AsQueryable();

// Now you can use Queryable operators — same syntax as EF Core
var results = queryable
    .Where(p => p.Price > 50)
    .OrderBy(p => p.Name)
    .Take(10)
    .ToList();
```

Looks identical to an EF Core query. The only difference — the expression tree is compiled and run against the in-memory list, not translated to SQL.

---

## Writing One Method That Works Over Both

This is the real power. Write a query method that accepts `IQueryable<T>` — and it works whether you pass an EF Core `DbSet` or a local list wrapped with `AsQueryable()`:

```csharp
// One method — works over any sequence
public IQueryable<Product> GetSellingProductsUnder(
    IQueryable<Product> source, decimal maxPrice)
{
    return source
        .Where(Product.IsSelling())
        .Where(p => p.Price < maxPrice)
        .OrderBy(p => p.Name);
}
```

```csharp
// Remote — runs as SQL in the database
var fromDb = GetSellingProductsUnder(context.Products, 100).ToList();

// Local — runs in memory against a cached list
var fromCache = GetSellingProductsUnder(cachedProducts.AsQueryable(), 100).ToList();
```

Exact same query logic. No duplication. The source decides where it runs.

---

## Why This Matters for Testing

In unit tests you don't want a real database — you want fast, isolated, in-memory data. But your repository methods are written against `IQueryable<T>`. With `AsQueryable()` you can test them without mocking EF Core at all:

```csharp
[Fact]
public void GetSellingProducts_FiltersDiscontinued()
{
    // Arrange — plain in-memory list, no DB needed
    var products = new List<Product>
    {
        new Product { Id = 1, Name = "Active",       IsDiscontinued = false,
                      LastSoldOn = DateTime.UtcNow.AddDays(-5),  Price = 50 },
        new Product { Id = 2, Name = "Discontinued", IsDiscontinued = true,
                      LastSoldOn = DateTime.UtcNow.AddDays(-5),  Price = 50 },
        new Product { Id = 3, Name = "Stale",        IsDiscontinued = false,
                      LastSoldOn = DateTime.UtcNow.AddDays(-60), Price = 50 },
    };

    // Act — wrap as IQueryable, run the real query method
    var results = GetSellingProductsUnder(products.AsQueryable(), 100).ToList();

    // Assert
    Assert.Single(results);
    Assert.Equal("Active", results[0].Name);
}
```

No database. No mocking. No EF Core setup. The actual query logic runs and is tested — fast and reliably.

---

## The Behaviour Difference — Where Expressions Are Evaluated

```csharp
// IQueryable from DbSet — expression tree → SQL
var db = context.Products.Where(p => p.Price > 50);
// → SELECT * FROM Products WHERE Price > 50

// IQueryable from AsQueryable() — expression tree → compiled → C# memory
var local = list.AsQueryable().Where(p => p.Price > 50);
// → loads nothing extra, filters the list in C#
```

The expression tree exists in both cases. The difference is what happens to it — EF Core translates it to SQL, LINQ to Objects compiles it to a delegate and runs it over the list.

---

## `AsQueryable()` vs `AsEnumerable()` — Opposites

```csharp
// AsEnumerable() — pulls DB data INTO memory, switches to Enumerable operators
context.Products
    .AsEnumerable()
    .Where(p => p.Price > 50);  // all rows loaded first, then filter in C#

// AsQueryable() — lifts a local list UP to the Queryable API
localList
    .AsQueryable()
    .Where(p => p.Price > 50);  // expression compiled, runs over list in C#
```

| | `AsEnumerable()` | `AsQueryable()` |
|---|---|---|
| Used on | `IQueryable` (DB query) | `IEnumerable` (local list) |
| Direction | DB → memory | memory → queryable API |
| Accepts operators | Delegates (`Func`) | Expression trees |
| Risk | Loads full table into RAM | None |

---

## A Complete Pattern — Source-Agnostic Service

```csharp
public class ProductService
{
    private readonly IQueryable<Product> _source;

    // Accepts a DbSet OR a local list — doesn't know which
    public ProductService(IQueryable<Product> source)
        => _source = source;

    public List<Product> GetCheapSellingProducts(decimal maxPrice)
        => _source
            .Where(Product.IsSelling())
            .Where(p => p.Price < maxPrice)
            .OrderBy(p => p.Name)
            .ToList();
}

// Production — real database
var service = new ProductService(context.Products);

// Test — in-memory list
var service = new ProductService(fakeProducts.AsQueryable());
```

The service is completely unaware of where data comes from. The query is identical either way.

---

## Summary

```
AsQueryable()   → wraps IEnumerable<T> as IQueryable<T>
                → expression tree API available on local sequences
                → expressions compiled and run in C# memory (not SQL)

Key use cases:
  → Write one query method, run it over DB or local list
  → Unit test IQueryable logic without a real database
  → Uniform API regardless of data source

AsQueryable()  ≠  AsEnumerable()
  AsEnumerable  → moves DB query INTO memory (fetches all data first)
  AsQueryable   → moves local list UP to queryable API (no extra data loaded)
```

---

# Expression Tree DOM — Node Types & Manual Construction

---

## The Implicit Conversion — What the Compiler Does for You

When you assign a lambda to `Expression<TDelegate>`, the compiler doesn't compile it into IL. Instead it **implicitly converts** it — building an expression tree object graph on your behalf.

```csharp
// Implicit conversion — compiler builds the tree automatically
Expression<Func<string, bool>> expr = s => s.Length > 5;
```

The same thing built **manually** produces an identical tree and can be cast to `Expression<TDelegate>` and used in EF Core exactly the same way:

```csharp
// Manual construction — identical result
ParameterExpression s    = Expression.Parameter(typeof(string), "s");
MemberExpression length  = Expression.Property(s, "Length");
ConstantExpression five  = Expression.Constant(5);
BinaryExpression body    = Expression.GreaterThan(length, five);

Expression<Func<string, bool>> expr =
    Expression.Lambda<Func<string, bool>>(body, s);

// Cast and use in EF Core — works identically to a lambda
var results = context.Products
    .Where((Expression<Func<Product, bool>>)manualExpr)
    .ToList();
```

The implicit conversion is syntactic sugar. The compiler generates the exact same object construction you'd write by hand.

---

## The Expression DOM — Every Node Is an Object

An expression tree is a tree of `Expression` objects. Think of it exactly like an XML DOM — each node has a type, children, and a `NodeType` property identifying what it represents. The static `Expression` class is the **factory** for creating nodes.

---

## `Expression` — The Base of Everything

All node types inherit from `Expression`. Every node exposes:

```csharp
expr.NodeType  // what kind of node: Add, Equal, Call, Lambda, Parameter, etc.
expr.Type      // the CLR type the expression evaluates to
```

The static `Expression` class creates all node types:

```csharp
Expression.Parameter(...)     // → ParameterExpression
Expression.Property(...)      // → MemberExpression
Expression.Constant(...)      // → ConstantExpression
Expression.GreaterThan(...)   // → BinaryExpression
Expression.AndAlso(...)       // → BinaryExpression
Expression.Lambda(...)        // → LambdaExpression
Expression.Call(...)          // → MethodCallExpression
Expression.Not(...)           // → UnaryExpression
```

---

## `ParameterExpression` — The Variable

Represents a named parameter — the `p` in `p => p.Price > 50`. It's the entry point of the tree — all property accesses start from here.

```csharp
ParameterExpression p = Expression.Parameter(typeof(Product), "p");

Console.WriteLine(p.Name);     // → "p"
Console.WriteLine(p.Type);     // → Product
Console.WriteLine(p.NodeType); // → Parameter
```

Every lambda has a **ParameterCollection** — a `ReadOnlyCollection<ParameterExpression>` holding all its parameters. A single-parameter lambda has one. A two-parameter lambda has two.

```csharp
Expression<Func<Product, decimal, bool>> expr =
    (p, minPrice) => p.Price > minPrice;

ParameterExpression param1 = expr.Parameters[0];  // p        : Product
ParameterExpression param2 = expr.Parameters[1];  // minPrice : decimal
```

The same `ParameterExpression` instance **must be shared** across every node in the tree that references that parameter. Two parameter expressions with the same name are still two different objects — EF Core will treat them as unrelated variables.

---

## `MemberExpression` — Property or Field Access

Represents accessing a property or field on an object — `p.Price`, `p.Name`, `s.Length`.

```csharp
ParameterExpression p     = Expression.Parameter(typeof(Product), "p");
MemberExpression    price = Expression.Property(p, nameof(Product.Price));
MemberExpression    name  = Expression.Property(p, nameof(Product.Name));

Console.WriteLine(price.Member.Name);  // → "Price"
Console.WriteLine(price.Expression);   // → p   (the object being accessed)
Console.WriteLine(price.NodeType);     // → MemberAccess
```

For nested access — `p.Category.Name`:

```csharp
MemberExpression category     = Expression.Property(p, nameof(Product.Category));
MemberExpression categoryName = Expression.Property(category, nameof(Category.Name));
// → represents p.Category.Name
```

---

## `ConstantExpression` — A Fixed Value

Represents a literal value — `50`, `"Widget"`, `true`, `null`.

```csharp
ConstantExpression fifty = Expression.Constant(50m, typeof(decimal));
ConstantExpression label = Expression.Constant("Widget", typeof(string));
ConstantExpression flag  = Expression.Constant(true, typeof(bool));
ConstantExpression none  = Expression.Constant(null, typeof(string));

Console.WriteLine(fifty.Value);    // → 50
Console.WriteLine(fifty.Type);     // → Decimal
Console.WriteLine(fifty.NodeType); // → Constant
```

When EF Core's translator encounters a `ConstantExpression`, it becomes a parameterised SQL value — `@p0 = 50` — safe from SQL injection.

---

## `BinaryExpression` — Two Operands, One Operator

Represents any operation taking a left and right operand:

```csharp
// Comparisons
Expression.GreaterThan(price, fifty)       // p.Price > 50
Expression.LessThanOrEqual(price, fifty)   // p.Price <= 50
Expression.Equal(name, label)              // p.Name == "Widget"
Expression.NotEqual(name, label)           // p.Name != "Widget"

// Logical
Expression.AndAlso(left, right)            // left && right
Expression.OrElse(left, right)             // left || right

// Arithmetic
Expression.Add(price, fifty)              // p.Price + 50
Expression.Multiply(price, fifty)         // p.Price * 50
```

```csharp
BinaryExpression comparison = Expression.GreaterThan(price, fifty);

Console.WriteLine(comparison.Left);       // → p.Price  (MemberExpression)
Console.WriteLine(comparison.Right);      // → 50       (ConstantExpression)
Console.WriteLine(comparison.NodeType);   // → GreaterThan
```

---

## `LambdaExpression` and `Expression<TDelegate>`

`LambdaExpression` is the root — it wraps the body and the ParameterCollection together. `Expression<TDelegate>` is the strongly typed subclass used in practice.

```csharp
ParameterExpression p    = Expression.Parameter(typeof(Product), "p");
MemberExpression price   = Expression.Property(p, nameof(Product.Price));
ConstantExpression fifty = Expression.Constant(50m, typeof(decimal));
BinaryExpression body    = Expression.GreaterThan(price, fifty);

// Untyped
LambdaExpression untyped = Expression.Lambda(body, p);

// Strongly typed — used with EF Core
Expression<Func<Product, bool>> typed =
    Expression.Lambda<Func<Product, bool>>(body, p);

Console.WriteLine(typed.Body);               // → (p.Price > 50)
Console.WriteLine(typed.Parameters[0].Name); // → "p"
Console.WriteLine(typed.NodeType);           // → Lambda
Console.WriteLine(typed.ReturnType);         // → Boolean
```

---

## Walking a Tree — Reading Every Node

```csharp
Expression<Func<Product, bool>> expr = p => p.Price > 50 && !p.IsDiscontinued;

// Root: Lambda
var lambda = expr;
Console.WriteLine(lambda.NodeType);          // → Lambda

// Body: AndAlso
var andAlso = (BinaryExpression)lambda.Body;
Console.WriteLine(andAlso.NodeType);         // → AndAlso

// Left: GreaterThan
var gt = (BinaryExpression)andAlso.Left;
var member = (MemberExpression)gt.Left;
Console.WriteLine(member.Member.Name);       // → "Price"
var constant = (ConstantExpression)gt.Right;
Console.WriteLine(constant.Value);           // → 50

// Right: Not
var not = (UnaryExpression)andAlso.Right;
Console.WriteLine(not.NodeType);             // → Not
```

The full tree for `p => p.Price > 50 && !p.IsDiscontinued`:

```
Lambda (p => ...)
└── Body: BinaryExpression (AndAlso)
    ├── Left:  BinaryExpression (GreaterThan)
    │          ├── Left:  MemberExpression (p.Price)
    │          │          └── ParameterExpression (p : Product)
    │          └── Right: ConstantExpression (50)
    └── Right: UnaryExpression (Not)
               └── Operand: MemberExpression (p.IsDiscontinued)
                            └── ParameterExpression (p : Product)
```

---

## The Full Node Type Reference

| Node Type | Class | Represents | Example |
|---|---|---|---|
| `Parameter` | `ParameterExpression` | A named variable | `p` |
| `MemberAccess` | `MemberExpression` | Property/field access | `p.Price` |
| `Constant` | `ConstantExpression` | Fixed value | `50`, `"Widget"` |
| `GreaterThan` | `BinaryExpression` | `>` | `p.Price > 50` |
| `AndAlso` | `BinaryExpression` | `&&` | `a && b` |
| `OrElse` | `BinaryExpression` | `\|\|` | `a \|\| b` |
| `Equal` | `BinaryExpression` | `==` | `p.Name == "x"` |
| `Not` | `UnaryExpression` | `!` | `!p.IsDiscontinued` |
| `Call` | `MethodCallExpression` | Method call | `s.Contains("x")` |
| `Lambda` | `LambdaExpression` | The whole lambda | `p => p.Price > 50` |
| `Convert` | `UnaryExpression` | Type cast | `(decimal)p.Id` |
| `New` | `NewExpression` | Constructor call | `new DateTime(...)` |

---

## Summary

```
Implicit conversion → lambda to Expression<TDelegate>
    compiler builds the tree automatically — same as manual construction
    manual version can be cast to Expression<TDelegate> and used in EF Core

Expression DOM — every node is a subclass of Expression:

ParameterExpression   → the variable (p)
                      → shared instance across the whole tree — not duplicated
MemberExpression      → property/field access (p.Price, p.Category.Name)
ConstantExpression    → a fixed value (50, "Widget") → SQL parameterised value
BinaryExpression      → two operands + operator (>, &&, ==, +, *)
UnaryExpression       → single operand (!, type cast)
MethodCallExpression  → method call (s.Contains("x"))
LambdaExpression      → the root — body + ParameterCollection
Expression<TDelegate> → strongly typed LambdaExpression — used with EF Core

ParameterCollection   → ReadOnlyCollection<ParameterExpression> on the lambda
                      → same instance must be referenced throughout the tree
```
