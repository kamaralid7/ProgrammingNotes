# Advanced C# — 09: Func and Action Delegates

> *"Two delegates to rule them all."*

---

## The Core Idea

`Action` and `Func` cover 95% of all delegate use cases. They treat methods as first-class values — store them, pass them, return them, compose them.

```
Action = "Do something, return nothing"  (side effects)
Func   = "Do something, give me a value" (computations)
```

---

## Action — Void Delegate

`Action` performs work and returns nothing.

```csharp
Action sayHello = () => Console.WriteLine("Hello!");
sayHello(); // Hello!

Action<string> greet        = name => Console.WriteLine($"Hey {name}!");
Action<string, int> repeat  = (msg, times) =>
{
    for (int i = 0; i < times; i++)
        Console.WriteLine(msg);
};

greet("Ali");            // Hey Ali!
repeat("C# is fun", 3); // prints 3 times
```

---

## Func — Value-Returning Delegate

`Func<T, TResult>` — the **last** type parameter is always the **return type**.

```csharp
Func<int>           getFortyTwo = () => 42;
Func<int, int>      square      = x => x * x;
Func<int, int, int> add         = (a, b) => a + b;
Func<string, int>   getLength   = s => s.Length;
Func<string, bool>  isLong      = s => s.Length > 10;

Console.WriteLine(getFortyTwo());         // 42
Console.WriteLine(square(5));             // 25
Console.WriteLine(add(3, 4));             // 7
Console.WriteLine(getLength("Hello"));    // 5
Console.WriteLine(isLong("Hello World")); // True
```

Reading a `Func` signature: *"takes these inputs, produces this output."*

---

## The Symmetry

```
Action<A, B, C>          →  void Method(A a, B b, C c)
Func<A, B, C, TResult>   →  TResult Method(A a, B b, C c)
```

`Action` is `Func` with no return value. That's it.

---

## Storing and Passing Behavior

Treating methods as first-class values:

```csharp
// Store in a variable — reassign anytime
Func<int, int> operation = x => x * 2;
operation = x => x + 100;

// Store in a list — build dynamic pipelines
var transforms = new List<Func<int, int>>
{
    x => x * 2,
    x => x + 10,
    x => x - 3
};

int value = 5;
foreach (var t in transforms)
    value = t(value);

Console.WriteLine(value); // ((5 * 2) + 10) - 3 = 17
```

---

## Func as a Lazy Value

`Func<T>` (no parameters) defers computation until you actually need the result:

```csharp
// Eager — runs immediately even if never used
string result = ExpensiveOperation();

// Lazy — only computes when called
Func<string> lazyResult = () => ExpensiveOperation();

if (needsData)
    Console.WriteLine(lazyResult()); // computed only now
```

The .NET `Lazy<T>` class uses this pattern internally.

---

## Action for Callbacks

```csharp
public void DownloadFile(
    string url,
    Action<string>    onComplete,
    Action<Exception> onError)
{
    try
    {
        string content = FetchContent(url);
        onComplete(content);
    }
    catch (Exception ex)
    {
        onError(ex);
    }
}

DownloadFile(
    "https://api.example.com/data",
    onComplete: content => Console.WriteLine($"Got {content.Length} chars"),
    onError:    ex      => Console.WriteLine($"Failed: {ex.Message}")
);
```

---

## Func for Strategies

Swap algorithms at runtime by swapping the `Func`:

```csharp
public class Sorter<T>
{
    private readonly Func<T, T, int> _compare;
    public Sorter(Func<T, T, int> compareStrategy) => _compare = compareStrategy;

    public List<T> Sort(List<T> items)
    {
        var result = new List<T>(items);
        result.Sort((a, b) => _compare(a, b));
        return result;
    }
}

var ascSorter  = new Sorter<int>((a, b) => a.CompareTo(b));
var descSorter = new Sorter<int>((a, b) => b.CompareTo(a));

var nums = new List<int> { 5, 1, 4, 2, 3 };
Console.WriteLine(string.Join(", ", ascSorter.Sort(nums)));  // 1, 2, 3, 4, 5
Console.WriteLine(string.Join(", ", descSorter.Sort(nums))); // 5, 4, 3, 2, 1
```

---

## Composing Func Chains

Since `Func` returns a value, you can feed output of one into the next:

```csharp
Func<string, string> trim    = s => s.Trim();
Func<string, string> upper   = s => s.ToUpper();
Func<string, string> exclaim = s => s + "!!!";

// Extension method for fluent composition
static Func<TIn, TOut> Then<TIn, TMid, TOut>(
    this Func<TIn, TMid> first,
    Func<TMid, TOut> second) => x => second(first(x));

var chain = trim.Then(upper).Then(exclaim);
Console.WriteLine(chain("  hello  ")); // HELLO!!!
```

---

## Method Groups vs Lambdas

Both work, but method groups are slightly more efficient:

```csharp
// Method group — points directly to existing method, no wrapper
Func<string, int> a = int.Parse;

// Lambda wrapping a method — extra indirection
Func<string, int> b = s => int.Parse(s);

// For performance-critical code, prefer method groups
var result  = list.Select(int.Parse);        // ✅ efficient
var result2 = list.Select(s => int.Parse(s)); // tiny overhead
```

---

## Mental Model

```
Action<Order>        = "process this order"        (a TODO item)
Func<Order, bool>    = "is this order valid?"       (a question)
Func<Order, Receipt> = "generate a receipt"         (a transformation)
```

---

## Summary

| | `Action` | `Func` |
|--|----------|--------|
| Returns | `void` | A value |
| Use for | Side effects, callbacks | Computations, transformations |
| Composable | Via multicast (`+=`) | Via chaining (`.Then()`) |
| Max params | 16 | 16 + 1 return |
| Lazy eval | `Action` called later | `Func<T>` called later |

---

*Previous: [08 - Generic Delegate Types](./08%20-%20Generic%20Delegate%20Types.md)*
*Next: [10 - Func and Action with ref, out and Pointers](./10%20-%20Func%20and%20Action%20with%20ref%2C%20out%20and%20Pointers.md)*
