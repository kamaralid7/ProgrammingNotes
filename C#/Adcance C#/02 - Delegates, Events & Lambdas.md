# Advanced C# — 02: Delegates, Events & Lambdas

---

## 1. What is a Delegate?

A **delegate** is a type-safe function pointer. It holds a reference to a method (or multiple methods) and can invoke them later.

Think of it as a variable that stores a method.

```csharp
// Step 1: Declare the delegate type
delegate int MathOperation(int a, int b);

// Step 2: Create methods that match the signature
int Add(int a, int b) => a + b;
int Multiply(int a, int b) => a * b;

// Step 3: Assign and invoke
MathOperation op = Add;
Console.WriteLine(op(3, 4)); // 7

op = Multiply;
Console.WriteLine(op(3, 4)); // 12
```

---

## 2. Built-in Delegates: Func, Action, Predicate

You rarely need to define custom delegates. C# provides generic built-in ones:

### `Action` — returns void

```csharp
Action greet = () => Console.WriteLine("Hello!");
greet(); // Hello!

Action<string, int> repeat = (msg, times) =>
{
    for (int i = 0; i < times; i++)
        Console.WriteLine(msg);
};
repeat("Hi", 3);
```

### `Func` — has a return value (last type param is return type)

```csharp
Func<int, int, int> add = (a, b) => a + b;
Console.WriteLine(add(5, 3)); // 8

Func<string, int> getLength = s => s.Length;
Console.WriteLine(getLength("Hello")); // 5

Func<string> getMessage = () => "Hello World";
Console.WriteLine(getMessage()); // Hello World
```

### `Predicate<T>` — returns bool (shorthand for `Func<T, bool>`)

```csharp
Predicate<int> isEven = n => n % 2 == 0;
Console.WriteLine(isEven(4));  // True
Console.WriteLine(isEven(7));  // False
```

### Comparison Table

| Delegate | Signature | Use Case |
|----------|-----------|----------|
| `Action` | `void Method(...)` | Fire and forget |
| `Func<T, TResult>` | `TResult Method(T...)` | Transformation / computation |
| `Predicate<T>` | `bool Method(T)` | Filtering / testing |

---

## 3. Multicast Delegates

A delegate can hold **multiple methods**. When invoked, all methods are called in order.

```csharp
Action logger = () => Console.WriteLine("Log to Console");
logger += () => Console.WriteLine("Log to File");
logger += () => Console.WriteLine("Log to Database");

logger();
// Output:
// Log to Console
// Log to File
// Log to Database

// Remove a method
Action fileLogger = () => Console.WriteLine("Log to File");
logger -= fileLogger; // removes exact match
```

> **Important:** With multicast delegates that return a value, only the **last** return value is captured.

```csharp
Func<int> counter = () => 1;
counter += () => 2;
counter += () => 3;

int result = counter(); // result = 3 (only last return value)
```

### Iterating Invocation List

```csharp
Func<int> counter = () => 1;
counter += () => 2;
counter += () => 3;

// Get all return values
foreach (Func<int> d in counter.GetInvocationList())
{
    Console.WriteLine(d()); // 1, 2, 3
}
```

---

## 4. Lambda Expressions

Lambdas are anonymous functions — inline method definitions.

### Expression Lambda (single expression)

```csharp
Func<int, int> square = x => x * x;
Func<int, int, int> add = (x, y) => x + y;
```

### Statement Lambda (block of code)

```csharp
Func<int, string> describe = x =>
{
    if (x > 0) return "Positive";
    if (x < 0) return "Negative";
    return "Zero";
};
```

### Lambdas as Arguments

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

// Filter with lambda
var evens = numbers.Where(n => n % 2 == 0).ToList();

// Transform with lambda
var squares = numbers.Select(n => n * n).ToList();

// Sort with lambda
numbers.Sort((a, b) => a.CompareTo(b));
```

---

## 5. Closures

A lambda can **capture variables** from its enclosing scope — this is called a **closure**.

```csharp
int multiplier = 3;
Func<int, int> triple = x => x * multiplier; // captures 'multiplier'

Console.WriteLine(triple(5)); // 15

multiplier = 10; // changing the captured variable
Console.WriteLine(triple(5)); // 50 — captured by REFERENCE, not value!
```

### The Classic Loop Closure Bug

```csharp
// BUG — all lambdas capture the same 'i' reference
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
{
    actions.Add(() => Console.WriteLine(i));
}
actions.ForEach(a => a()); // prints 3, 3, 3 (not 0, 1, 2!)

// FIX — capture a local copy
for (int i = 0; i < 3; i++)
{
    int copy = i; // new variable each iteration
    actions.Add(() => Console.WriteLine(copy));
}
actions.ForEach(a => a()); // prints 0, 1, 2
```

---

## 6. Events

Events are a **publish-subscribe** mechanism built on top of delegates. A class can raise events, and other classes subscribe to them.

### Declaring and Raising Events

```csharp
public class Button
{
    // Declare event using EventHandler delegate
    public event EventHandler? Clicked;

    // Method to raise the event
    protected virtual void OnClicked()
    {
        Clicked?.Invoke(this, EventArgs.Empty); // null-safe invocation
    }

    public void SimulateClick() => OnClicked();
}
```

### Subscribing to Events

```csharp
var button = new Button();

// Subscribe with method
button.Clicked += Button_Clicked;

// Subscribe with lambda
button.Clicked += (sender, e) => Console.WriteLine("Lambda handler fired!");

button.SimulateClick();
// Output:
// Button was clicked!
// Lambda handler fired!

// Unsubscribe
button.Clicked -= Button_Clicked;

void Button_Clicked(object? sender, EventArgs e)
{
    Console.WriteLine("Button was clicked!");
}
```

---

## 7. Custom EventArgs

Pass data with your events using a custom `EventArgs` class.

```csharp
// Custom EventArgs
public class OrderEventArgs : EventArgs
{
    public int OrderId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PlacedAt { get; init; }
}

// Publisher
public class OrderService
{
    public event EventHandler<OrderEventArgs>? OrderPlaced;

    public void PlaceOrder(int id, decimal amount)
    {
        // ... process order ...
        OnOrderPlaced(new OrderEventArgs
        {
            OrderId = id,
            Amount = amount,
            PlacedAt = DateTime.UtcNow
        });
    }

    protected virtual void OnOrderPlaced(OrderEventArgs e)
    {
        OrderPlaced?.Invoke(this, e);
    }
}

// Subscriber
var service = new OrderService();
service.OrderPlaced += (sender, e) =>
{
    Console.WriteLine($"Order #{e.OrderId} placed: ${e.Amount} at {e.PlacedAt}");
};

service.PlaceOrder(101, 249.99m);
// Output: Order #101 placed: $249.99 at 2024-01-15 10:30:00
```

---

## 8. event Keyword vs Plain Delegate

| Feature | `event` | Plain `delegate` |
|---------|---------|-----------------|
| Subscribe (`+=`) | ✅ Allowed | ✅ Allowed |
| Unsubscribe (`-=`) | ✅ Allowed | ✅ Allowed |
| Direct assignment (`=`) | ❌ Only inside class | ✅ Allowed from outside |
| Direct invocation | ❌ Only inside class | ✅ Allowed from outside |

Events are **encapsulated** — only the declaring class can raise them or reset them.

```csharp
public class Publisher
{
    public event Action? OnDataReceived;      // event — safe
    public Action? OnDataReceivedDelegate;    // plain delegate — unsafe

    public void Raise()
    {
        OnDataReceived?.Invoke();          // OK
        OnDataReceivedDelegate?.Invoke();  // OK
    }
}

var pub = new Publisher();
pub.OnDataReceived += () => Console.WriteLine("A");
pub.OnDataReceived += () => Console.WriteLine("B");

// DANGER with plain delegate — outsider can wipe all subscriptions!
pub.OnDataReceivedDelegate = () => Console.WriteLine("Replaced everything!");

// With event — this would be a COMPILE ERROR:
// pub.OnDataReceived = () => Console.WriteLine("Compile error!");
```

---

## 9. Weak Events (Memory Leak Prevention)

Subscribing to events can cause **memory leaks** — the publisher holds a reference to the subscriber, preventing GC.

```csharp
// PROBLEM: subscriber stays alive as long as publisher is alive
button.Clicked += myHandler; // button holds reference to this object

// SOLUTION 1: Always unsubscribe when done
button.Clicked -= myHandler;

// SOLUTION 2: Use WeakReference pattern (advanced)
public class WeakEventHandler<TEventArgs>
{
    private readonly WeakReference _handlerRef;

    public WeakEventHandler(EventHandler<TEventArgs> handler)
    {
        _handlerRef = new WeakReference(handler.Target);
    }
}
```

---

## 10. Anonymous Methods (Legacy Syntax)

Before lambdas (pre-C# 3.0), you used `delegate` keyword for anonymous methods. You may see this in older code.

```csharp
// Old syntax (C# 2.0)
button.Click += delegate(object sender, EventArgs e)
{
    Console.WriteLine("Clicked!");
};

// Modern equivalent (C# 3.0+)
button.Click += (sender, e) => Console.WriteLine("Clicked!");
```

---

## 11. Delegates as Strategies (Strategy Pattern)

Delegates allow injecting behavior from outside — a lightweight alternative to interfaces for the Strategy pattern.

```csharp
public class DataProcessor
{
    private readonly Func<string, string> _transform;
    private readonly Predicate<string> _filter;

    public DataProcessor(Func<string, string> transform, Predicate<string> filter)
    {
        _transform = transform;
        _filter = filter;
    }

    public IEnumerable<string> Process(IEnumerable<string> data)
    {
        return data
            .Where(item => _filter(item))
            .Select(item => _transform(item));
    }
}

// Usage
var processor = new DataProcessor(
    transform: s => s.ToUpper(),
    filter: s => s.Length > 3
);

var results = processor.Process(new[] { "Hi", "Hello", "World", "C#" });
// ["HELLO", "WORLD"]
```

---

## 12. Delegate Covariance and Contravariance

Delegates support **covariance** (return type) and **contravariance** (parameter type).

```csharp
// Covariance — delegate can return a MORE DERIVED type
Func<Animal> getAnimal = () => new Dog(); // Dog is-a Animal ✅

// Contravariance — delegate can accept a LESS DERIVED parameter
Action<Dog> trainDog = (Animal a) => Console.WriteLine("Train"); // ✅
```

---

## 13. Real-World Example: Event-Driven Logger

```csharp
public enum LogLevel { Info, Warning, Error }

public class LogEventArgs : EventArgs
{
    public LogLevel Level { get; init; }
    public string Message { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public class Logger
{
    public event EventHandler<LogEventArgs>? MessageLogged;

    public void Log(LogLevel level, string message)
    {
        MessageLogged?.Invoke(this, new LogEventArgs
        {
            Level = level,
            Message = message
        });
    }
}

// Wire up multiple handlers
var logger = new Logger();

logger.MessageLogged += (_, e) =>
    Console.WriteLine($"[{e.Level}] {e.Message}");

logger.MessageLogged += (_, e) =>
{
    if (e.Level == LogLevel.Error)
        File.AppendAllText("errors.log", $"{e.Timestamp}: {e.Message}\n");
};

logger.Log(LogLevel.Info, "Application started");
logger.Log(LogLevel.Error, "Database connection failed");
```

---

## Summary

| Concept | Key Point |
|---------|-----------|
| **Delegate** | Type-safe function pointer |
| **Action** | Delegate returning void |
| **Func<T, TResult>** | Delegate returning a value |
| **Predicate<T>** | Delegate returning bool |
| **Multicast** | One delegate, many methods |
| **Lambda** | Anonymous inline function |
| **Closure** | Lambda capturing outer variables |
| **Event** | Encapsulated delegate (publish/subscribe) |
| **EventArgs** | Data passed with events |
| `?.Invoke()` | Null-safe event invocation |

---

*Previous: [01 - Async/Await & Tasks](./01%20-%20Async_Await%20%26%20Tasks.md)*
*Next: [03 - Generics & Constraints](./03%20-%20Generics%20%26%20Constraints.md)*
