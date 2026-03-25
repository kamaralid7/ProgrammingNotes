# Advanced C# — 03: Higher-Order Functions

---

## What is a Higher-Order Function?

A **higher-order function** is a function that either:
- **Takes** a function as a parameter, or
- **Returns** a function as its result (or both)

It is the foundation of functional-style programming in C#. Delegates and lambdas are what make it possible.

```
Normal function:        input  →  output
Higher-order (take):    (input + function)  →  output
Higher-order (return):  input  →  function
Higher-order (both):    function  →  new function
```

---

## 1. Functions That Take Functions

Pass a `Func` or `Action` as an argument to inject behavior from outside.

```csharp
// Higher-order function — accepts a condition function
static List<T> Filter<T>(List<T> items, Predicate<T> condition)
{
    var result = new List<T>();
    foreach (var item in items)
        if (condition(item))
            result.Add(item);
    return result;
}

// Usage — plug in any behavior
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

var evens    = Filter(numbers, n => n % 2 == 0);             // [2, 4, 6, 8]
var big      = Filter(numbers, n => n > 5);                  // [6, 7, 8]
var oddSmall = Filter(numbers, n => n % 2 != 0 && n < 5);   // [1, 3]
```

The function `Filter` doesn't know or care *what* the condition is — the caller decides.

---

## 2. Functions That Return Functions

A function **manufactures** a new function based on inputs.

```csharp
// Returns a new Func — a "multiplier factory"
static Func<int, int> CreateMultiplier(int factor)
{
    return x => x * factor; // captures 'factor' in a closure
}

var double = CreateMultiplier(2);
var triple = CreateMultiplier(3);
var tenX   = CreateMultiplier(10);

Console.WriteLine(double(5));  // 10
Console.WriteLine(triple(5));  // 15
Console.WriteLine(tenX(5));    // 50
```

Each call to `CreateMultiplier` creates a **brand new function** with `factor` baked in via closure.

---

## 3. Practical Example: Validator Factory

Instead of writing separate validators for every rule, you factory them:

```csharp
static Func<string, bool> MinLength(int min) =>
    s => s.Length >= min;

static Func<string, bool> MaxLength(int max) =>
    s => s.Length <= max;

static Func<string, bool> Contains(string keyword) =>
    s => s.Contains(keyword, StringComparison.OrdinalIgnoreCase);

// Compose validators into a pipeline
var validators = new List<Func<string, bool>>
{
    MinLength(3),
    MaxLength(20),
    Contains("@") // basic email check
};

bool IsValid(string input) => validators.All(v => v(input));

Console.WriteLine(IsValid("hi"));               // False (too short)
Console.WriteLine(IsValid("ali@example.com"));  // True
Console.WriteLine(IsValid("noemail"));          // False (no @)
```

---

## 4. Function Composition

Wire two functions end-to-end — the output of `f` becomes the input of `g`.

```csharp
static Func<T, TResult> Compose<T, TMiddle, TResult>(
    Func<T, TMiddle> f,
    Func<TMiddle, TResult> g)
{
    return x => g(f(x));
}

Func<string, string> trim   = s => s.Trim();
Func<string, string> upper  = s => s.ToUpper();
Func<string, int>    getLen = s => s.Length;

var trimThenUpper = Compose(trim, upper);
var pipeline      = Compose(trimThenUpper, getLen);

Console.WriteLine(trimThenUpper("  hello "));  // "HELLO"
Console.WriteLine(pipeline("  hello "));       // 5
```

---

## 5. Memoization — Cache Expensive Results

A higher-order function that wraps *any* function and caches its results automatically.

```csharp
static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> func)
    where T : notnull
{
    var cache = new Dictionary<T, TResult>();

    return input =>
    {
        if (!cache.TryGetValue(input, out var result))
        {
            result = func(input);   // compute only on first call
            cache[input] = result;
        }
        return result;
    };
}

// Wrap any expensive function
Func<int, long> slowFib = null!;
slowFib = n => n <= 1 ? n : slowFib(n - 1) + slowFib(n - 2);

var fastFib = Memoize(slowFib);

Console.WriteLine(fastFib(10)); // 55 — computed
Console.WriteLine(fastFib(10)); // 55 — returned from cache instantly
```

---

## 6. Retry — Higher-Order for Resilience

Wrap any async operation in retry logic without changing the operation itself.

```csharp
static async Task<T> WithRetry<T>(
    Func<Task<T>> operation,
    int maxAttempts = 3,
    TimeSpan? delay = null)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying...");
            await Task.Delay(delay ?? TimeSpan.FromSeconds(1));
        }
    }
    return await operation(); // last attempt — let it throw
}

// Use with ANY async operation
var html = await WithRetry(() => httpClient.GetStringAsync("https://api.example.com"));
var data = await WithRetry(() => db.QueryAsync("SELECT ..."), maxAttempts: 5);
```

---

## 7. Timing / Benchmarking Wrapper

Wrap any function to measure how long it takes — without modifying the function itself.

```csharp
static Func<T, TResult> WithTiming<T, TResult>(
    Func<T, TResult> func,
    string label = "")
{
    return input =>
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = func(input);
        sw.Stop();
        Console.WriteLine($"{label} took {sw.ElapsedMilliseconds}ms");
        return result;
    };
}

Func<int, long> heavyComputation = n => Enumerable.Range(1, n).Sum(x => (long)x);
var timedComputation = WithTiming(heavyComputation, "Sum");

Console.WriteLine(timedComputation(10_000_000));
// Sum took 42ms
// 50000005000000
```

---

## 8. Partial Application

"Pre-fill" some arguments of a function, creating a new function that needs fewer arguments.

```csharp
// General partial application for a two-argument function
static Func<T2, TResult> Partial<T1, T2, TResult>(
    Func<T1, T2, TResult> func,
    T1 firstArg)
{
    return second => func(firstArg, second);
}

Func<string, string, string> greet = (greeting, name) => $"{greeting}, {name}!";

// Pre-fill the greeting
var sayHello = Partial(greet, "Hello");
var sayHi    = Partial(greet, "Hi");

Console.WriteLine(sayHello("Ali"));    // Hello, Ali!
Console.WriteLine(sayHi("Ali"));       // Hi, Ali!
Console.WriteLine(sayHello("World"));  // Hello, World!
```

---

## 9. Combining Multiple Higher-Order Patterns

A real pipeline that composes memoization, timing, and retry together:

```csharp
Func<string, Task<string>> fetch = url => httpClient.GetStringAsync(url);

// Layer higher-order wrappers
var timedFetch    = WithTiming(fetch, "HTTP fetch");
var resilientFetch = url => WithRetry(() => timedFetch(url), maxAttempts: 3);

// Now calling resilientFetch gives you timing + retry for free
string result = await resilientFetch("https://api.example.com/data");
```

---

## Summary

| Pattern | What It Does |
|---------|-------------|
| **Takes a function** | Inject behavior — `Filter(list, condition)` |
| **Returns a function** | Factory behavior — `CreateMultiplier(3)` |
| **Wraps a function** | Add cross-cutting concerns — `Memoize`, `WithRetry`, `WithTiming` |
| **Composes functions** | Chain transformations — `Compose(trim, upper)` |
| **Partial application** | Pre-fill arguments — `Partial(greet, "Hello")` |

Higher-order functions eliminate repetition, make behavior injectable, and are the foundation of LINQ, middleware pipelines, and the Strategy/Decorator design patterns.

---

*Previous: [02 - Delegates, Events & Lambdas](./02%20-%20Delegates%2C%20Events%20%26%20Lambdas.md)*
*Next: [04 - Plugin Methods with Delegates](./04%20-%20Plugin%20Methods%20with%20Delegates.md)*
