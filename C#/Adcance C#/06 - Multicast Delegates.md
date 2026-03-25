# Advanced C# — 06: Multicast Delegates

> *"One delegate. Many methods. One call. All fire."*

---

## What is a Multicast Delegate?

Every delegate in C# is already a multicast delegate — it can hold **a chain of method references**, not just one. When you invoke it, all methods in the chain are called in order.

```csharp
Action greet = () => Console.WriteLine("Hello from Lambda 1");
greet        += () => Console.WriteLine("Hello from Lambda 2");
greet        += () => Console.WriteLine("Hello from Lambda 3");

greet();
// Hello from Lambda 1
// Hello from Lambda 2
// Hello from Lambda 3
```

---

## Adding and Removing Methods

```csharp
void LogToConsole(string msg) => Console.WriteLine($"[Console] {msg}");
void LogToFile(string msg)    => File.AppendAllText("log.txt", msg + "\n");
void LogToDb(string msg)      => Console.WriteLine($"[DB] {msg}");

Action<string> logger = LogToConsole;
logger += LogToFile;
logger += LogToDb;

logger("App started"); // calls all three

// Remove a specific method
logger -= LogToFile;

logger("App running"); // calls LogToConsole and LogToDb only
```

> **Note:** `-=` removes the **last matching** entry in the chain. If the same method was added twice, only one occurrence is removed.

---

## The Invocation List

Internally, a multicast delegate stores an array of delegates. You can access it directly:

```csharp
Action<string> logger = LogToConsole;
logger += LogToFile;
logger += LogToDb;

Delegate[] list = logger.GetInvocationList();
Console.WriteLine(list.Length); // 3

foreach (var d in list)
{
    Console.WriteLine($"Method: {d.Method.Name}, Target: {d.Target ?? "static"}");
}
// Method: LogToConsole, Target: static
// Method: LogToFile,    Target: static
// Method: LogToDb,      Target: static
```

---

## Return Values — Only the Last One Counts

With multicast delegates that return a value, only the **last** method's return is captured. All methods still run — you just lose the earlier results.

```csharp
Func<int> counter = () => 1;
counter           += () => 2;
counter           += () => 3;

int result = counter(); // result = 3 — only last return value
```

### Capturing ALL Return Values

Use `GetInvocationList()` to collect every return value:

```csharp
Func<int> counter = () => 1;
counter           += () => 2;
counter           += () => 3;

var allResults = counter
    .GetInvocationList()
    .Cast<Func<int>>()
    .Select(d => d())
    .ToList();

Console.WriteLine(string.Join(", ", allResults)); // 1, 2, 3
```

---

## Exception Handling in Multicast Delegates

By default, if **one method throws**, the remaining methods in the chain **do not run**:

```csharp
Action chain = () => Console.WriteLine("Step 1");
chain        += () => throw new Exception("Step 2 failed!");
chain        += () => Console.WriteLine("Step 3"); // never reached!

try { chain(); }
catch (Exception ex) { Console.WriteLine(ex.Message); }
// Step 1
// Step 2 failed!
// Step 3 is NEVER printed
```

### Resilient Multicast — Call All, Collect Errors

Use `GetInvocationList()` to call each delegate individually and handle exceptions per-method:

```csharp
Action chain = () => Console.WriteLine("Step 1");
chain        += () => throw new Exception("Step 2 exploded");
chain        += () => Console.WriteLine("Step 3");

var errors = new List<Exception>();

foreach (Action d in chain.GetInvocationList())
{
    try { d(); }
    catch (Exception ex) { errors.Add(ex); }
}

if (errors.Any())
    Console.WriteLine($"{errors.Count} error(s) occurred.");

// Step 1
// Step 3
// 1 error(s) occurred.
```

---

## Mixing Instance and Static Targets

A single multicast delegate can hold a mix of instance and static method targets:

```csharp
class Logger
{
    public string Name;
    public Logger(string name) => Name = name;
    public void Log(string msg) => Console.WriteLine($"[{Name}] {msg}");
}

static void GlobalLog(string msg) => Console.WriteLine($"[GLOBAL] {msg}");

var fileLogger = new Logger("FileLogger");
var dbLogger   = new Logger("DbLogger");

Action<string> log = fileLogger.Log;  // instance target
log                += dbLogger.Log;   // instance target
log                += GlobalLog;      // static target (Target = null)

log("System ready");
// [FileLogger] System ready
// [DbLogger]   System ready
// [GLOBAL]     System ready

// Inspect each entry
foreach (var d in log.GetInvocationList())
    Console.WriteLine($"{d.Method.Name} → Target: {d.Target?.ToString() ?? "static"}");
```

---

## Thread Safety — Multicast Delegates and Null Checks

Delegates are **immutable**. When you do `+=` or `-=`, a new delegate object is created. This means the original is untouched — which is good for thread safety, but requires careful null checking.

```csharp
public class EventPublisher
{
    // Could be null if no subscribers
    public event Action<string>? DataReceived;

    public void Publish(string data)
    {
        // SAFE — capture local copy before null check
        // Prevents race condition between null-check and invocation
        var handler = DataReceived;
        handler?.Invoke(data);

        // EQUIVALENT modern syntax:
        DataReceived?.Invoke(data); // also thread-safe in C# 6+
    }
}
```

---

## Real-World Pattern: Plugin Notification System

Multicast delegates are the natural backbone of notification/hook systems:

```csharp
public class OrderPipeline
{
    public Action<Order>?  OnOrderCreated;
    public Action<Order>?  OnOrderShipped;
    public Action<Order, Exception>? OnOrderFailed;

    public void ProcessOrder(Order order)
    {
        try
        {
            Console.WriteLine($"Processing order #{order.Id}...");
            OnOrderCreated?.Invoke(order);

            ShipOrder(order);
            OnOrderShipped?.Invoke(order);
        }
        catch (Exception ex)
        {
            OnOrderFailed?.Invoke(order, ex);
        }
    }

    private void ShipOrder(Order order) { /* ... */ }
}

// Register multiple handlers — all fire automatically
var pipeline = new OrderPipeline();

pipeline.OnOrderCreated += order => Console.WriteLine($"  → Email sent for #{order.Id}");
pipeline.OnOrderCreated += order => Console.WriteLine($"  → Inventory reserved for #{order.Id}");
pipeline.OnOrderShipped += order => Console.WriteLine($"  → Tracking created for #{order.Id}");
pipeline.OnOrderFailed  += (order, ex) => Console.WriteLine($"  → Alert raised: {ex.Message}");

pipeline.ProcessOrder(new Order { Id = 42 });
```

---

## Equality and Removal Rules

```csharp
// Named method removal — works correctly
Action a = MyMethod;
a += MyMethod;
a += MyMethod;
Console.WriteLine(a.GetInvocationList().Length); // 3

a -= MyMethod; // removes ONE occurrence (the last added)
Console.WriteLine(a.GetInvocationList().Length); // 2

// Lambda removal — does NOT work
Action b = () => Console.WriteLine("hi");
b += () => Console.WriteLine("hi"); // different object even if same code
b -= () => Console.WriteLine("hi"); // removes NOTHING — different instance

Console.WriteLine(b.GetInvocationList().Length); // still 2!
```

> **Rule:** You can only reliably remove a delegate if you hold a reference to the **exact same instance** you added.

```csharp
// CORRECT way to handle lambda removal
Action<string> handler = msg => Console.WriteLine(msg);
logger += handler;  // add by reference
logger -= handler;  // remove by same reference — works
```

---

## Summary

| Concept | Key Point |
|---------|-----------|
| Multicast | Every delegate can hold multiple methods |
| `+=` / `-=` | Add or remove methods from the chain |
| Invocation order | Methods called in the order they were added |
| Return values | Only last return value is captured |
| Exception behavior | One throw stops the rest — use `GetInvocationList()` to be resilient |
| `GetInvocationList()` | Access each delegate entry individually |
| Thread safety | Capture to local variable before null check + invoke |
| Lambda removal | Doesn't work — hold a named reference instead |
| Instance targets | Each entry tracks its own target object |

---

*Previous: [05 - Open & Closed Generics + LINQ Internals](./05%20-%20Open%20%26%20Closed%20Generics%20%2B%20LINQ%20Internals.md)*
*Next: [07 - Instance and Static Method Targets](./07%20-%20Instance%20and%20Static%20Method%20Targets.md)*
