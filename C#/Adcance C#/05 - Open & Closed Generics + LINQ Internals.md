# Advanced C# — 05: Open & Closed Generics + LINQ Internals ⚗️🧵

> *"The most exciting thing about generics isn't what you can do with them — it's what the CLR is silently doing behind your back."*

---

## 🔬 PART 1: Open & Closed Generics

### The Two Kinds of Generic Types

Most developers use generics every day without realizing there are **two different kinds** living inside the runtime.

| Kind | Description | Example |
|------|-------------|---------|
| **Open** | Type parameter not yet filled in — a template | `Box<>`, `List<>` |
| **Closed** | Type parameter resolved — a real, usable type | `Box<int>`, `List<string>` |

```csharp
// OPEN generic — T is not filled in yet, can't instantiate
class Box<T>
{
    public T Value { get; set; }
}

// CLOSED generics — T is resolved, real types exist in memory
Box<int>    intBox    = new();
Box<string> stringBox = new();
```

---

### Watching the CLR Close Generics in Real Time

```csharp
Type openType  = typeof(Box<>);       // open — note the <>
Type closedInt = typeof(Box<int>);
Type closedStr = typeof(Box<string>);

Console.WriteLine(openType.IsGenericTypeDefinition);  // True  — it's a template
Console.WriteLine(closedInt.IsGenericTypeDefinition); // False — it's a real type

Console.WriteLine(closedInt == closedStr); // False — literally different types!
Console.WriteLine(openType  == closedInt); // False

// Close a generic AT RUNTIME dynamically
Type closedDouble = openType.MakeGenericType(typeof(double));
object box = Activator.CreateInstance(closedDouble);
Console.WriteLine(box.GetType()); // Box`1[System.Double]
```

This is how DI containers, serializers, and ORMs dynamically create typed instances from open generic definitions.

---

### The CLR's Secret Optimization 🤫

For **value types**, the CLR generates unique native code per closed type:

```
Box<int>    → separate native code (int = 4 bytes, laid out directly in memory)
Box<double> → separate native code (double = 8 bytes)
Box<long>   → separate native code
```

For **reference types**, the CLR reuses the **same native code** — because all references are the same size (pointer width):

```
Box<string>   ┐
Box<object>   ├── ALL share the same native code internally
Box<MyClass>  ┘
```

This is why `List<string>` and `List<MyClass>` don't double your binary size — they share machine code under the hood.

---

### Open Generic Methods

You can get open generic *methods* too and close them at runtime:

```csharp
class Converter
{
    public static TOut Convert<TIn, TOut>(TIn input, Func<TIn, TOut> converter)
        => converter(input);
}

// Get the open generic method (unresolved type params)
var openMethod = typeof(Converter).GetMethod(nameof(Converter.Convert));

// Close it at runtime with specific types
var closedMethod = openMethod!.MakeGenericMethod(typeof(string), typeof(int));

// Invoke dynamically
var result = closedMethod.Invoke(null, new object[]
{
    "12345",
    (Func<string, int>)int.Parse
});

Console.WriteLine(result); // 12345
```

DI containers do exactly this when resolving `IRepository<Order>` — they find the open method, close it to the right type, and invoke it.

---

### Practical Use: Generic Factory

```csharp
public class GenericFactory
{
    private readonly Dictionary<Type, Type> _map = new();

    public void Register<TInterface, TImplementation>()
        where TImplementation : TInterface
    {
        _map[typeof(TInterface)] = typeof(TImplementation);
    }

    public T Create<T>()
    {
        if (!_map.TryGetValue(typeof(T), out var implType))
            throw new InvalidOperationException($"No registration for {typeof(T).Name}");

        return (T)Activator.CreateInstance(implType)!;
    }
}

// Usage
var factory = new GenericFactory();
factory.Register<ILogger, FileLogger>();
factory.Register<ICache, MemoryCache>();

var logger = factory.Create<ILogger>(); // returns a FileLogger
```

---

## 🧵 PART 2: LINQ Internals

### Deferred Execution — The "Lazy" Trick

LINQ queries **don't run when you write them**. They build a chain of instructions and only execute when you iterate.

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };

// NOTHING runs here — just building a description of work
var query = numbers
    .Where(n =>  { Console.WriteLine($"  Filter: {n}"); return n > 2; })
    .Select(n => { Console.WriteLine($"  Select: {n}"); return n * 10; });

Console.WriteLine("Query built. Iterating now...");

foreach (var item in query) // <-- execution starts HERE
    Console.WriteLine($"Got: {item}");
```

Output:
```
Query built. Iterating now...
  Filter: 1
  Filter: 2
  Filter: 3
  Select: 3
Got: 30
  Filter: 4
  Select: 4
Got: 40
  Filter: 5
  Select: 5
Got: 50
```

One element travels through the **entire pipeline** before the next one starts. This is the **pull model** — each `MoveNext()` pulls one item through all operators.

---

### How It Works — Iterator State Machines

Every LINQ operator is a state machine under the hood. Here's what `Where` essentially does:

```csharp
// This is roughly what Where() does internally
static IEnumerable<T> MyWhere<T>(IEnumerable<T> source, Func<T, bool> predicate)
{
    foreach (var item in source)
    {
        if (predicate(item))
            yield return item; // suspends here, resumes on next MoveNext()
    }
}

// And Select:
static IEnumerable<TResult> MySelect<T, TResult>(
    IEnumerable<T> source, Func<T, TResult> selector)
{
    foreach (var item in source)
        yield return selector(item);
}
```

`yield return` compiles into a state machine class. Each call to `MoveNext()` resumes from where it left off — that's how the pipeline stays lazy.

---

### The Chain Is Nested Enumerables (Russian Dolls 🪆)

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };

// What the LINQ chain actually looks like internally:
// WhereEnumerable(
//     SelectEnumerable(
//         numbers,            ← innermost source
//         x => x * 2
//     ),
//     x => x > 4
// )

var query = numbers
    .Select(x => x * 2) // wraps 'numbers' in SelectEnumerable
    .Where(x => x > 4); // wraps that in WhereEnumerable
```

Each operator wraps the previous like nesting dolls. Enumerating the outermost pulls from the one inside, all the way to the raw source.

---

### When Deferred Execution Bites You 🐍

```csharp
var data = new List<int> { 1, 2, 3 };
var query = data.Where(x => x > 1); // deferred — not executed yet

data.Add(4); // mutate the source AFTER building the query
data.Add(5);

foreach (var x in query)
    Console.WriteLine(x); // 2, 3, 4, 5 — sees the new items!
```

The query sees the **live state** at enumeration time — not when it was defined.

---

### Immediate Execution Operators

These **force** the query to run now and materialize results:

```csharp
List<int> results = numbers.Where(x => x > 2).ToList();  // full run, store in memory
int count         = numbers.Where(x => x > 2).Count();   // full run, return scalar
int first         = numbers.First(x => x > 2);           // stops at first match
bool any          = numbers.Any(x => x > 2);             // stops at first match
```

---

### Expression Trees — LINQ to SQL's Secret Weapon 🌳

When you use LINQ against a database (`IQueryable<T>`), your lambda is **not compiled to IL**. It's compiled to an **expression tree** — a runtime object describing the code as *data*.

```csharp
// IEnumerable — lambda is a real compiled C# method (black box)
IEnumerable<int> memQuery = list.Where(x => x > 5);

// IQueryable — lambda becomes an expression TREE (readable data structure)
IQueryable<Product> dbQuery = dbContext.Products.Where(p => p.Price > 100);
//                                                       ^^^^^^^^^^^^^^^^
//                                            Expression<Func<Product, bool>>
//                                            NOT Func<Product, bool>
```

The expression tree looks like this at runtime:

```
BinaryExpression (GreaterThan)
├── MemberExpression  (p.Price)
│   └── ParameterExpression (p : Product)
└── ConstantExpression (100)
```

Entity Framework **reads this tree** and translates it to SQL:

```sql
SELECT * FROM Products WHERE Price > 100
```

If it were a plain `Func<>`, EF would have no idea what SQL to generate — it's just a black-box method pointer.

---

### Building Expression Trees Manually

```csharp
using System.Linq.Expressions;

// Build: x => x * x
var param  = Expression.Parameter(typeof(int), "x");
var body   = Expression.Multiply(param, param);
var lambda = Expression.Lambda<Func<int, int>>(body, param);

// Inspect it as DATA before compiling
Console.WriteLine(lambda);        // x => (x * x)
Console.WriteLine(body.NodeType); // Multiply
Console.WriteLine(body.Left);     // x
Console.WriteLine(body.Right);    // x

// Compile to a real delegate
Func<int, int> square = lambda.Compile();
Console.WriteLine(square(5)); // 25
```

This is how ORMs, AutoMapper, and serializers work — they *read your lambda as data*, then translate it to something else.

---

### IEnumerable vs IQueryable — The Key Divide

| | `IEnumerable<T>` | `IQueryable<T>` |
|--|-----------------|-----------------|
| Executes | In memory (C#) | Remote (SQL, API, etc.) |
| Lambda type | `Func<T, bool>` | `Expression<Func<T, bool>>` |
| Filter happens | After fetching all rows | In the database (WHERE clause) |
| Use for | In-memory collections | Databases, remote sources |

```csharp
// DANGER — fetches ALL products into memory first, then filters in C#
var bad = dbContext.Products
    .AsEnumerable()          // forces switch to IEnumerable!
    .Where(p => p.Price > 100);

// GOOD — WHERE clause runs in SQL, only matching rows travel over the wire
var good = dbContext.Products
    .Where(p => p.Price > 100); // stays as IQueryable
```

---

## Summary

| Concept | Key Insight |
|---------|-------------|
| Open generic | A type template — `Box<>` — can't be instantiated |
| Closed generic | A resolved type — `Box<int>` — real type in memory |
| `MakeGenericType()` | Close a generic at runtime dynamically |
| CLR value types | Each closed type gets unique native code |
| CLR reference types | All closed types share the same native code |
| Deferred execution | LINQ queries run when iterated, not when defined |
| Iterator state machine | `yield return` = lazy pull model |
| Expression trees | Lambda as data — readable, translatable |
| `IQueryable<T>` | Expression trees → SQL translation |
| `IEnumerable<T>` | Compiled delegates → in-memory execution |

---

*Previous: [04 - Plugin Methods with Delegates](./04%20-%20Plugin%20Methods%20with%20Delegates.md)*
*Next: [06 - Multicast Delegates](./06%20-%20Multicast%20Delegates.md)*
