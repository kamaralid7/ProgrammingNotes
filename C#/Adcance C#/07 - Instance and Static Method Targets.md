# Advanced C# — 07: Instance and Static Method Targets

> *"A delegate doesn't just remember what to call — it remembers who to call it on."*

---

## The Two Kinds of Method Targets

When you assign a method to a delegate, the delegate stores two things:
- **Which method** to call (`Method`)
- **What object** to call it on (`Target`)

For static methods, there is no object — `Target` is `null`.
For instance methods, `Target` holds a reference to the specific object.

---

## Static Method Target

A static method belongs to the type, not any object. The delegate stores only the method pointer.

```csharp
class MathHelper
{
    public static int Double(int x) => x * 2;
    public static int Square(int x) => x * x;
}

Func<int, int> op = MathHelper.Double;

Console.WriteLine(op(5));          // 10
Console.WriteLine(op.Target);      // null — no object captured
Console.WriteLine(op.Method.Name); // "Double"

op = MathHelper.Square;
Console.WriteLine(op(5));          // 25
```

Static method delegates are **lightweight** — no object reference, no memory overhead beyond the method pointer itself.

---

## Instance Method Target

An instance method belongs to a specific object. The delegate captures both the method *and* the object it operates on.

```csharp
class Greeter
{
    private readonly string _prefix;
    public Greeter(string prefix) => _prefix = prefix;
    public string Greet(string name) => $"{_prefix}, {name}!";
}

var formal = new Greeter("Good day");
var casual = new Greeter("Hey");

Func<string, string> formalGreet = formal.Greet;
Func<string, string> casualGreet = casual.Greet;

Console.WriteLine(formalGreet("Ali")); // Good day, Ali!
Console.WriteLine(casualGreet("Ali")); // Hey, Ali!

// Inspect the captured target
Console.WriteLine(formalGreet.Target == formal); // True
Console.WriteLine(casualGreet.Target == casual); // True
Console.WriteLine(formalGreet.Method.Name);      // "Greet"
```

Each delegate holds its own object reference — completely independent.

---

## Live State: Delegates See Object Mutations

Because an instance delegate holds a **reference** to the object (not a copy), changes to the object are visible through the delegate:

```csharp
class Counter
{
    public int Count = 0;
    public int Increment(int by) { Count += by; return Count; }
}

var c = new Counter();
Func<int, int> inc = c.Increment;

Console.WriteLine(inc(5));   // 5   (c.Count = 5)
Console.WriteLine(inc(3));   // 8   (c.Count = 8)
Console.WriteLine(c.Count);  // 8   — same object, same state
```

---

## Inspecting Target and Method at Runtime

```csharp
class Processor
{
    public string Name { get; init; } = "";
    public string Process(string s) => s.ToUpper();
    public static string Clean(string s) => s.Trim();
}

var p = new Processor { Name = "MyProcessor" };

Func<string, string> instanceDel = p.Process;
Func<string, string> staticDel   = Processor.Clean;

// Instance delegate
Console.WriteLine(instanceDel.Target);          // Processor object
Console.WriteLine(instanceDel.Target == p);     // True
Console.WriteLine(instanceDel.Method.Name);     // "Process"
Console.WriteLine(instanceDel.Method.IsStatic); // False

// Static delegate
Console.WriteLine(staticDel.Target);            // null
Console.WriteLine(staticDel.Method.Name);       // "Clean"
Console.WriteLine(staticDel.Method.IsStatic);   // True
```

---

## Multicast: Mixing Instance and Static Targets

A single multicast delegate can hold a mix of both:

```csharp
class Logger
{
    public string Name;
    public Logger(string name) => Name = name;
    public void Log(string msg) => Console.WriteLine($"[{Name}] {msg}");
}

static void GlobalLog(string msg) => Console.WriteLine($"[GLOBAL] {msg}");

var logger1 = new Logger("FileLogger");
var logger2 = new Logger("DbLogger");

Action<string> log = logger1.Log;  // instance: target = logger1
log                += logger2.Log; // instance: target = logger2
log                += GlobalLog;   // static:   target = null

log("App started");
// [FileLogger] App started
// [DbLogger]   App started
// [GLOBAL]     App started

// Each invocation list entry has its own Target
foreach (var d in log.GetInvocationList())
    Console.WriteLine($"  {d.Method.Name} → {d.Target?.ToString() ?? "static"}");
```

---

## Memory Implication — Instance Delegates Keep Objects Alive

Because an instance delegate holds a reference to its target object, the **garbage collector cannot collect** that object as long as the delegate is alive.

```csharp
public class DataFeed
{
    public event Action<string>? DataReceived;
}

public class Dashboard
{
    public Dashboard(DataFeed feed)
    {
        // feed now holds a strong reference to this Dashboard via the delegate
        feed.DataReceived += HandleData;
    }

    private void HandleData(string data) => Console.WriteLine(data);
}

var feed = new DataFeed();
var dash = new Dashboard(feed);

dash = null; // setting to null is NOT enough!
// GC cannot collect Dashboard — feed's event still holds the reference

// FIX: always unsubscribe when done
feed.DataReceived -= dash.HandleData;
```

---

## Static vs Instance — Side-by-Side Comparison

```csharp
class Counter
{
    private int _count = 0;
    public int Increment(int by) { _count += by; return _count; } // instance
    public static int Pure(int x, int by) => x + by;              // static — no state
}

var c1 = new Counter();
var c2 = new Counter();

Func<int, int> inc1 = c1.Increment; // tied to c1's state
Func<int, int> inc2 = c2.Increment; // tied to c2's state

Console.WriteLine(inc1(5));  // 5  (c1._count = 5)
Console.WriteLine(inc1(3));  // 8  (c1._count = 8)
Console.WriteLine(inc2(10)); // 10 (c2._count = 10, independent of c1)
```

---

## Summary Table

| | Static Target | Instance Target |
|--|--------------|-----------------|
| `delegate.Target` | `null` | Reference to the object |
| `delegate.Method.IsStatic` | `true` | `false` |
| Memory overhead | Minimal — method pointer only | Holds object alive in memory |
| Shared state | None (pure) | Shares object's mutable state |
| GC impact | None | Prevents GC if delegate lives long |
| Best for | Utilities, pure functions | Stateful, OOP-style methods |

---

*Previous: [06 - Multicast Delegates](./06%20-%20Multicast%20Delegates.md)*
*Next: See [toExplore.md](./toExplore.md) for what to tackle next!*
