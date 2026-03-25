# Advanced C# — 08: Generic Delegate Types

> *"Write one delegate. Make it work for every type."*

---

## What is a Generic Delegate?

A generic delegate has type parameters — instead of one delegate per type, one delegate works for all of them.

```csharp
// Without generics — a new delegate for every type
delegate int    TransformInt(int x);
delegate string TransformString(string x);
delegate double TransformDouble(double x);  // forever...

// With generics — one delegate, any type
delegate TResult Transformer<T, TResult>(T input);

Transformer<int, string>    intToStr = n => n.ToString();
Transformer<string, int>    strToInt = s => s.Length;
Transformer<double, double> sqrtIt   = Math.Sqrt;

Console.WriteLine(intToStr(42));      // "42"
Console.WriteLine(strToInt("Hello")); // 5
Console.WriteLine(sqrtIt(16));        // 4
```

---

## The Built-ins Are Generic Delegates

`Func`, `Action`, and `Predicate` are generic delegates defined in the BCL:

```csharp
// How they're actually defined
public delegate TResult Func<T, TResult>(T arg);
public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
// ... up to Func<T1...T16, TResult>

public delegate void Action<T>(T arg);
public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
// ... up to Action<T1...T16>

public delegate bool Predicate<T>(T obj);
```

Every `Func<int, string>` you write is closing a generic delegate — same concept as `List<int>` closing `List<T>`.

---

## Generic Delegates With Constraints

Add type constraints just like generic classes:

```csharp
// Only works with comparable types
delegate int Comparer<T>(T a, T b) where T : IComparable<T>;

Comparer<int>    compareInts    = (a, b) => a.CompareTo(b);
Comparer<string> compareStrings = (a, b) => a.CompareTo(b);

Console.WriteLine(compareInts(3, 5));        // -1
Console.WriteLine(compareStrings("b", "a")); //  1
```

---

## Covariance — `out T`

Mark a type parameter `out` when it only appears in **return** (output) position. This allows substituting a more derived type:

```csharp
class Animal { public string Name = "Animal"; }
class Dog : Animal { }

Func<Dog>    getDog    = () => new Dog();
Func<Animal> getAnimal = getDog; // ✅ Func<Dog> → Func<Animal>
                                 // safe: caller asked for Animal, gets Dog — fine

Animal a = getAnimal();
```

`Func<TResult>` is declared with `out TResult`, so this assignment just works.

---

## Contravariance — `in T`

Mark a type parameter `in` when it only appears in **parameter** (input) position. This allows substituting a less derived type:

```csharp
Action<Animal> handleAnimal = a => Console.WriteLine(a.Name);
Action<Dog>    handleDog    = handleAnimal; // ✅ Action<Animal> → Action<Dog>
                                            // safe: if it handles any Animal, it handles a Dog

handleDog(new Dog());
```

`Action<T>` is declared with `in T`, so this assignment just works.

---

## Declaring Your Own Variant Generic Delegate

```csharp
// COVARIANT — T only in return position
delegate T Producer<out T>();

// CONTRAVARIANT — T only in parameter position
delegate void Consumer<in T>(T input);

// INVARIANT — T in both positions, no variance modifier allowed
delegate T Transformer<T>(T input);
```

Usage:

```csharp
Producer<Dog>    dogProducer    = () => new Dog();
Producer<Animal> animalProducer = dogProducer;  // ✅ covariance

Consumer<Animal> animalConsumer = a => Console.WriteLine(a.Name);
Consumer<Dog>    dogConsumer    = animalConsumer; // ✅ contravariance
```

---

## Building a Generic Pipeline With Generic Delegates

A fully type-safe pipeline that transforms any type to any other:

```csharp
public class Pipeline<TIn, TOut>
{
    private readonly List<Func<object, object>> _steps = new();

    public Pipeline<TIn, TNext> AddStep<TNext>(Func<TOut, TNext> step)
    {
        _steps.Add(input => step((TOut)input)!);
        return new Pipeline<TIn, TNext>(_steps);
    }

    public TOut Execute(TIn input)
    {
        object current = input!;
        foreach (var step in _steps)
            current = step(current);
        return (TOut)current;
    }

    private Pipeline(List<Func<object, object>> steps) => _steps = steps;
    public Pipeline() { }
}

// string → int → double → string  (fully type-safe)
var pipeline = new Pipeline<string, string>()
    .AddStep<int>(s    => s.Length)
    .AddStep<double>(n => Math.Sqrt(n))
    .AddStep<string>(d => $"√{d:F2}");

Console.WriteLine(pipeline.Execute("Hello"));       // √2.24
Console.WriteLine(pipeline.Execute("Hello World")); // √3.32
```

---

## Generic Type-Safe Event Bus

```csharp
public class EventBus
{
    private readonly Dictionary<Type, Delegate> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        _handlers[type] = _handlers.ContainsKey(type)
            ? Delegate.Combine(_handlers[type], handler)
            : handler;
    }

    public void Publish<TEvent>(TEvent evt)
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var handler))
            ((Action<TEvent>)handler)(evt);
    }
}

record OrderPlaced(int OrderId, decimal Amount);
record UserLoggedIn(string Username);

var bus = new EventBus();
bus.Subscribe<OrderPlaced>(e  => Console.WriteLine($"Order #{e.OrderId}: ${e.Amount}"));
bus.Subscribe<OrderPlaced>(e  => Console.WriteLine($"Sending receipt for #{e.OrderId}"));
bus.Subscribe<UserLoggedIn>(e => Console.WriteLine($"Welcome back, {e.Username}!"));

bus.Publish(new OrderPlaced(101, 49.99m));
// Order #101: $49.99
// Sending receipt for #101

bus.Publish(new UserLoggedIn("Ali"));
// Welcome back, Ali!
```

---

## Summary

| Concept | Key Point |
|---------|-----------|
| Generic delegate | One definition, any type — same as generics on classes |
| Constraints | `where T : IComparable<T>` works on delegates too |
| `out T` covariance | T in return only — allows narrower type to substitute |
| `in T` contravariance | T in parameter only — allows broader type to substitute |
| Invariant | T in both positions — no variance, no substitution |
| Built-in `Func`/`Action` | Already generic delegates with `out`/`in` variance |

---

*Previous: [07 - Instance and Static Method Targets](./07%20-%20Instance%20and%20Static%20Method%20Targets.md)*
*Next: [09 - Func and Action Delegates](./09%20-%20Func%20and%20Action%20Delegates.md)*
