# Standard Dispose Semantics, Close and Stop

## Standard Dispose Semantics

"Standard Dispose semantics" is the agreed-upon set of rules for how `Dispose()` should behave. Think of it as the etiquette of resource cleanup — a contract between your class and whoever uses it.

### Rule 1 — Calling `Dispose()` multiple times must be safe (Idempotency)

No matter how many times `Dispose()` is called, it must never throw, crash, or behave differently after the first call. Every subsequent call should silently do nothing.

```csharp
public void Dispose()
{
    if (_disposed) return; // safe to call multiple times
    _stream.Dispose();
    _disposed = true;
}
```

This property is called **idempotency**. It matters because in complex code with nested `using` blocks or try/finally chains, `Dispose()` can accidentally get called more than once.

### Rule 2 — After disposal, any method call must throw `ObjectDisposedException`

Once disposed, the object is dead. Any meaningful operation on it should throw `ObjectDisposedException` immediately — not silently fail, not return stale data, not behave unpredictably.

```csharp
public void Write(string data)
{
    if (_disposed)
        throw new ObjectDisposedException(nameof(FileLogger));

    _writer.Write(data);
}
```

This is the "no zombie objects" rule. A disposed object should loudly announce it's dead rather than quietly misbehave. The one exception is `Dispose()` itself — it must remain callable without throwing (Rule 1).

### Rule 3 — `Dispose()` must never throw exceptions

If something goes wrong during cleanup — say, flushing a network stream fails because the connection already dropped — `Dispose()` should swallow the error silently. Throwing from `Dispose()` is dangerous because:

- It can mask the *original* exception that caused the `using` block to exit in the first place
- It makes `using` statements unreliable as a cleanup mechanism

```csharp
public void Dispose()
{
    if (_disposed) return;
    try
    {
        _networkStream.Flush();
    }
    catch { /* best-effort — swallow silently */ }
    finally
    {
        _networkStream.Dispose();
        _disposed = true;
    }
}
```

### Rule 4 — `Dispose()` is deterministic

Unlike the garbage collector, which runs on its own schedule, `Dispose()` runs **immediately** — the moment you call it or the `using` block exits. This is the core promise: you decide *when* resources are freed, not the runtime. This is called **deterministic finalization**.

### Rule 5 — If you have a finalizer, `Dispose()` must suppress it

```csharp
public void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}
```

Without `GC.SuppressFinalize`, the GC queues your object for finalization even after `Dispose()` already cleaned everything up — wasted effort and a performance cost.

### Rule 6 — Ownership determines responsibility

If `X` created and owns `Y`, then `X.Dispose()` must call `Y.Dispose()`. If `Y` was passed in from outside (injected), `X` generally should not dispose it — because the caller may still be using it.

---

## Close() and Stop()

Some types expose `Close()` or `Stop()` alongside `Dispose()`. These are not the same thing, and the distinction matters.

### Close()

`Close()` signals that you are **done using the resource for now** — but the object itself may still be reusable. It's a soft shutdown.

The clearest example is `SqlConnection`:

```csharp
var connection = new SqlConnection(connectionString);
connection.Open();

// ... do work ...

connection.Close();   // returns to the connection pool — still reusable
connection.Open();    // works fine — connection was only "closed", not destroyed

connection.Dispose(); // hard teardown — removed from pool, fully released
```

With `SqlConnection`, `Close()` returns the underlying TCP connection to the **connection pool** so it can be reused by the next caller. `Dispose()` removes it from the pool entirely.

For many other BCL types like `StreamReader` and `FileStream`, `Close()` is just an alias for `Dispose()` — they call the same internal method. Microsoft added `Close()` because it reads more naturally for streams and connections, not because it does anything different.

```csharp
// For StreamReader, these are equivalent:
reader.Close();
reader.Dispose();
```

The practical rule: **prefer `Dispose()` via a `using` statement** unless you have a specific reason to keep the object alive and reopen the resource later.

### Stop()

`Stop()` appears on types that represent **ongoing activity** — timers, background workers, servers, watchers. It pauses or ends the activity, but the object stays alive and can often be restarted.

```csharp
var timer = new System.Timers.Timer(1000);
timer.Elapsed += (s, e) => Console.WriteLine("tick");
timer.Start();

Thread.Sleep(5000);

timer.Stop();   // stops firing — timer object still exists
timer.Start();  // can restart — object is not disposed

timer.Dispose(); // now the timer is truly cleaned up
```

`Stop()` is about **behaviour** (stop doing the thing), while `Dispose()` is about **resources** (release what you're holding). A stopped timer still holds its OS handle. A disposed timer has released everything.

### The Relationship Between the Three

```
Stop()    → Pauses activity. Object still alive. Resources still held.
Close()   → Ends current session. Object may be reusable. Resources may be returned to a pool.
Dispose() → Final shutdown. Object is dead. All resources released.
```

They are not mutually exclusive. It is perfectly valid — and sometimes necessary — to call all three:

```csharp
timer.Stop();    // stop firing
timer.Close();   // close any internal handles (if applicable)
timer.Dispose(); // final cleanup
```

Though in most everyday usage, a single `using` with `Dispose()` is all you need.
