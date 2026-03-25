# Advanced C# — 01: Async/Await & Tasks

---

## 1. Why Asynchronous Programming?

Blocking threads is expensive. In web servers, desktop apps, and I/O-heavy code, waiting synchronously wastes CPU resources. Async/await lets you write non-blocking code that *looks* synchronous.

---

## 2. The Basics Refresher

```csharp
// Synchronous — blocks the thread
string data = File.ReadAllText("file.txt");

// Asynchronous — frees the thread while waiting
string data = await File.ReadAllTextAsync("file.txt");
```

- `async` marks a method as asynchronous
- `await` suspends the method until the awaited task completes
- The thread is released back to the thread pool during the wait

---

## 3. Task and Task<T>

`Task` represents an ongoing operation. `Task<T>` represents an operation that returns a value.

```csharp
// Returns Task<string>
public async Task<string> FetchDataAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}

// Returns Task (no return value)
public async Task SaveAsync(string content)
{
    await File.WriteAllTextAsync("output.txt", content);
}
```

### Task States
| State | Meaning |
|-------|---------|
| `Created` | Task created, not started |
| `Running` | Actively executing |
| `RanToCompletion` | Finished successfully |
| `Faulted` | Threw an exception |
| `Canceled` | Was cancelled |

---

## 4. ConfigureAwait(false)

By default, `await` captures the current **SynchronizationContext** and resumes on it (e.g., UI thread). Use `ConfigureAwait(false)` in library code to avoid deadlocks and improve performance.

```csharp
// Library code — don't need to resume on original context
public async Task<string> GetJsonAsync(string url)
{
    using var client = new HttpClient();
    string json = await client.GetStringAsync(url).ConfigureAwait(false);
    return json;
}
```

> **Rule of thumb:** Use `ConfigureAwait(false)` in library/infrastructure code. Don't use it in UI code or ASP.NET Core (it doesn't have a SynchronizationContext anyway).

---

## 5. Parallel Async Operations

### Run multiple tasks concurrently with Task.WhenAll

```csharp
var task1 = FetchDataAsync("https://api.example.com/a");
var task2 = FetchDataAsync("https://api.example.com/b");
var task3 = FetchDataAsync("https://api.example.com/c");

// All three run in parallel
string[] results = await Task.WhenAll(task1, task2, task3);
```

### Task.WhenAny — proceed when the first completes

```csharp
var tasks = new[] { task1, task2, task3 };
Task<string> firstDone = await Task.WhenAny(tasks);
string result = await firstDone;
```

### Difference Between WhenAll and Sequential Await

```csharp
// Sequential: 3 seconds total (1+1+1)
var r1 = await GetAsync("url1"); // waits 1s
var r2 = await GetAsync("url2"); // waits 1s
var r3 = await GetAsync("url3"); // waits 1s

// Parallel: ~1 second total
var r1r2r3 = await Task.WhenAll(GetAsync("url1"), GetAsync("url2"), GetAsync("url3"));
```

---

## 6. CancellationToken

Pass a `CancellationToken` to allow callers to cancel long-running operations.

```csharp
public async Task<string> FetchWithCancellationAsync(string url, CancellationToken ct)
{
    using var client = new HttpClient();
    // Pass token to the underlying call
    return await client.GetStringAsync(url, ct);
}

// Caller side
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5)); // auto-cancel after 5s

try
{
    string result = await FetchWithCancellationAsync("https://api.example.com", cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled.");
}
```

### Checking Cancellation Manually

```csharp
public async Task ProcessItemsAsync(List<int> items, CancellationToken ct)
{
    foreach (var item in items)
    {
        ct.ThrowIfCancellationRequested(); // throws OperationCanceledException
        await ProcessOneAsync(item, ct);
    }
}
```

---

## 7. Exception Handling in Async Code

```csharp
// Single task
try
{
    await FetchDataAsync();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP error: {ex.Message}");
}

// Multiple tasks with WhenAll — AggregateException
try
{
    await Task.WhenAll(task1, task2, task3);
}
catch (Exception ex) // catches the FIRST exception
{
    // To get all exceptions:
    var allExceptions = new[] { task1, task2, task3 }
        .Where(t => t.IsFaulted)
        .Select(t => t.Exception!.InnerException!);
}
```

---

## 8. ValueTask for Performance

`ValueTask<T>` avoids heap allocation when the result is often available synchronously (e.g., from cache).

```csharp
private Dictionary<string, string> _cache = new();

public async ValueTask<string> GetCachedAsync(string key)
{
    if (_cache.TryGetValue(key, out var value))
        return value; // synchronous path — no heap allocation

    var result = await FetchFromDbAsync(key);
    _cache[key] = result;
    return result;
}
```

> **Use `ValueTask` when:** The method frequently returns synchronously. **Use `Task` for:** General async methods, public APIs.

---

## 9. Async Streams (IAsyncEnumerable<T>)

Introduced in C# 8. Lets you stream data asynchronously with `yield return`.

```csharp
// Producer
public async IAsyncEnumerable<int> GenerateNumbersAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    for (int i = 0; i < 100; i++)
    {
        await Task.Delay(100, ct); // simulate async work
        yield return i;
    }
}

// Consumer
await foreach (int number in GenerateNumbersAsync())
{
    Console.WriteLine(number);
}
```

---

## 10. Common Async Pitfalls

### ❌ async void — Never use except for event handlers

```csharp
// BAD — exceptions are unobserved, can't be awaited
public async void LoadDataBad()
{
    await FetchAsync(); // if this throws, the app crashes
}

// GOOD
public async Task LoadDataGood()
{
    await FetchAsync();
}

// Exception: event handlers
private async void Button_Click(object sender, EventArgs e)
{
    await LoadDataGood(); // wrap async work in a Task method
}
```

### ❌ Deadlock with .Result or .Wait()

```csharp
// BAD — causes deadlock in UI / ASP.NET contexts
string result = FetchDataAsync().Result;
string result = FetchDataAsync().GetAwaiter().GetResult(); // also bad

// GOOD — always await
string result = await FetchDataAsync();
```

### ❌ Not awaiting fire-and-forget tasks

```csharp
// BAD — exceptions are swallowed
_ = Task.Run(() => DoWork()); // still bad if you want to know when it finishes

// BETTER — use a fire-and-forget helper if needed
FireAndForget(DoWorkAsync());

static async void FireAndForget(Task task)
{
    try { await task; }
    catch (Exception ex) { Logger.Log(ex); }
}
```

---

## 11. Task.Run — Offloading CPU-bound Work

Use `Task.Run` for CPU-bound work to avoid blocking the calling thread.

```csharp
// CPU-bound computation offloaded to thread pool
int result = await Task.Run(() =>
{
    return Enumerable.Range(1, 10_000_000).Sum();
});
```

> Don't use `Task.Run` for I/O-bound work — use async I/O methods directly.

---

## 12. Progress Reporting

```csharp
public async Task ProcessFilesAsync(IProgress<int> progress, CancellationToken ct)
{
    var files = Directory.GetFiles("path");
    for (int i = 0; i < files.Length; i++)
    {
        ct.ThrowIfCancellationRequested();
        await ProcessFileAsync(files[i]);
        progress?.Report((i + 1) * 100 / files.Length); // report percentage
    }
}

// Usage
var progress = new Progress<int>(percent =>
{
    Console.WriteLine($"Progress: {percent}%");
    // Update UI on the captured synchronization context
});

await ProcessFilesAsync(progress, CancellationToken.None);
```

---

## Summary

| Concept | Key Point |
|---------|-----------|
| `async/await` | Non-blocking, reads like sync code |
| `Task.WhenAll` | Run multiple tasks in parallel |
| `CancellationToken` | Always support cancellation |
| `ConfigureAwait(false)` | Use in library code, avoid deadlocks |
| `ValueTask<T>` | Optimize hot paths that return synchronously |
| `IAsyncEnumerable<T>` | Stream data asynchronously |
| `async void` | Only for event handlers |
| `.Result` / `.Wait()` | Never use in async contexts |

---

*Next: [02 - Delegates, Events & Lambdas](./02%20-%20Delegates%2C%20Events%20%26%20Lambdas.md)*
