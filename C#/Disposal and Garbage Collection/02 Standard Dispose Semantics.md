# Standard Dispose Semantics in C# 12

"Standard Dispose semantics" is the set of **rules and expectations** that the .NET community and Microsoft have agreed upon for how `Dispose()` should behave. Think of it as the etiquette around resource cleanup — a social contract between your class and whoever uses it.

---

## The Rules

### 1. Calling `Dispose()` more than once must be safe

No matter how many times someone calls `Dispose()`, it should never throw an exception or crash. The second, third, tenth call should simply do nothing.

```csharp
public void Dispose()
{
    if (_disposed) return; // idempotent — safe to call multiple times
    _stream.Dispose();
    _disposed = true;
}
```

This property is called **idempotency**. It matters because in complex code with try/finally blocks or `using` nesting, `Dispose()` can accidentally be called more than once.

---

### 2. After disposal, using the object must throw `ObjectDisposedException`

Once disposed, the object is dead. Any method call on it (other than `Dispose()` itself) should immediately throw `ObjectDisposedException` — not silently fail or return garbage data.

```csharp
public void Write(string data)
{
    if (_disposed)
        throw new ObjectDisposedException(nameof(FileLogger), "Cannot write to a disposed logger.");

    _writer.Write(data);
}
```

This is the "no zombie objects" rule. A disposed object should loudly announce it's dead rather than quietly misbehave.

---

### 3. `Dispose()` should never throw exceptions

If cleanup fails internally (e.g. flushing a network stream fails because the connection dropped), `Dispose()` should **swallow the error silently**. Throwing from `Dispose()` is considered very bad form because:

- It can mask the *original* exception that caused the `using` block to exit
- It makes `using` statements unreliable

```csharp
public void Dispose()
{
    if (_disposed) return;
    try
    {
        _networkStream.Flush(); // might fail — that's okay
    }
    catch { /* swallow — best effort cleanup */ }
    finally
    {
        _networkStream.Dispose();
        _disposed = true;
    }
}
```

---

### 4. `Dispose()` is deterministic — it runs *now*, not "eventually"

Unlike the garbage collector (which runs whenever it wants), `Dispose()` runs **immediately** when you call it or when the `using` block exits. This is the whole point — you get control over *when* resources are freed.

This is why it's called **deterministic finalization** as opposed to the GC's non-deterministic collection.

---

### 5. If you have a finalizer, `Dispose()` must suppress it

If your class has a finalizer (`~MyClass()`), calling `Dispose()` should tell the GC to skip it:

```csharp
public void Dispose()
{
    Dispose(true);
    GC.SuppressFinalize(this); // "GC, don't bother — we already cleaned up"
}
```

Without this, the GC queues your object for finalization *after* `Dispose()` already ran — wasted work and a performance hit.

---

### 6. `Dispose()` should not free memory directly

`Dispose()` is for **unmanaged resources** — file handles, network connections, OS handles. Don't try to null out managed object references inside `Dispose()` thinking it'll speed up garbage collection. It won't. Let the GC do its job for managed memory.

---

### 7. If X owns Y, X.Dispose() MUST call Y.Dispose()

If object `X` owns object `Y`, and both implement `IDisposable`, then `X.Dispose()` is responsible for calling `Y.Dispose()`. The runtime has no concept of ownership — it won't walk your object's fields and dispose them automatically.

```csharp
public class ReportWriter : IDisposable
{
    private FileStream _file;
    private StreamWriter _writer;
    private bool _disposed = false;

    public void Dispose()
    {
        if (_disposed) return;

        _writer?.Dispose(); // YOU must do this explicitly
        _file?.Dispose();   // YOU must do this explicitly
        _disposed = true;
    }
}
```

**Dispose in reverse order of creation** — later-created objects may depend on earlier ones, so tear them down first.

The inverse is equally important: if `X` did **not** create `Y` (i.e. `Y` was injected via constructor), `X` should **not** dispose `Y` — because `X` doesn't own it, and the real owner may still need it.

**Real-world example — `DeflateStream` wrapping `FileStream`:**

```csharp
// DeflateStream owns the FileStream — disposing DeflateStream disposes FileStream too
using var deflate = new DeflateStream(
    new FileStream("data.gz", FileMode.Create),
    CompressionMode.Compress
);

deflate.Write(data);
// One using → DeflateStream.Dispose() → FileStream.Dispose() → OS releases file handle
```

One `using`, two disposals — because each owner cleaned up what it owned.

---

## Full Example — All Rules Applied

```csharp
public class DatabaseSession : IDisposable
{
    private SqlConnection _connection;
    private bool _disposed = false;

    public DatabaseSession(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public SqlDataReader Query(string sql)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DatabaseSession)); // Rule 2

        return _connection.CreateCommand().ExecuteReader();
    }

    public void Dispose()
    {
        if (_disposed) return; // Rule 1 — idempotent

        try
        {
            _connection.Close();
            _connection.Dispose(); // Rule 7 — owner disposes what it owns
        }
        catch { /* Rule 3 — never throw from Dispose */ }

        _disposed = true;
    }
}
```

---

## Summary

| Rule | In Plain English |
|------|-----------------|
| Idempotent | Calling `Dispose()` twice must be safe and harmless |
| Zombie check | After disposal, method calls must throw `ObjectDisposedException` |
| Silent cleanup | `Dispose()` must never throw exceptions |
| Deterministic | Runs immediately — not "eventually" like the GC |
| Suppress finalizer | Call `GC.SuppressFinalize(this)` if you have a finalizer |
| Memory is GC's job | Don't null out managed fields — let the GC handle memory |
| Ownership chain | If you own it, you dispose it — in reverse creation order |

> **Be predictable, be safe, be quiet.** The consumer of your class should never have to worry about what happens when they dispose it.
