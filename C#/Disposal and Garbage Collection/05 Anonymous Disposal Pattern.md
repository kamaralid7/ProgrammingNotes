# The Anonymous Disposal Pattern

## The Problem — Clumsy Paired APIs

Imagine a class that fires events, and you need a way to temporarily suppress those events while you make a batch of changes. The obvious API looks like this:

```csharp
class Foo
{
    private int _suspendCount;

    public void SuspendEvents() => _suspendCount++;
    public void ResumeEvents()  => _suspendCount--;

    private void FireSomeEvent()
    {
        if (_suspendCount == 0)
            Console.WriteLine("Event fired!");
    }
}
```

And usage looks like this:

```csharp
foo.SuspendEvents();
DoSomething();
foo.ResumeEvents();
```

This works, but it is clumsy for one critical reason — the caller must always remember to call both methods, in the right order, even when things go wrong. If `DoSomething()` throws an exception, `ResumeEvents()` is never called, and events stay suppressed forever. That is a silent, hard-to-trace bug.

```csharp
foo.SuspendEvents();
DoSomething();         // throws — execution jumps past ResumeEvents()
foo.ResumeEvents();    // never reached
// events are now permanently suppressed
```

You could work around it with a try/finally, but that is extra ceremony pushed onto every caller. The API is fragile by design.

---

## Stage 1 — Push the Responsibility onto the Implementer

The better approach is to use a `using` block. Instead of exposing two separate methods, `SuspendEvents()` returns an `IDisposable` token. When the token is disposed, it automatically calls the resume logic. The `using` statement guarantees disposal — even on exception.

```csharp
using (foo.SuspendEvents())
{
    DoSomething(); // exception or not — ResumeEvents() will be called
}
```

This pushes the complexity onto whoever implements `SuspendEvents()` — which is exactly where it belongs. The caller's code becomes clean and safe. The implementer handles the details once.

The implementation introduces a small private token class:

```csharp
class Foo
{
    private int _suspendCount;

    public IDisposable SuspendEvents()
    {
        _suspendCount++;
        return new SuspendToken(this);
    }

    private void ResumeEvents() => _suspendCount--;

    private void FireSomeEvent()
    {
        if (_suspendCount == 0)
            Console.WriteLine("Event fired!");
    }

    private class SuspendToken : IDisposable
    {
        private Foo _foo;

        public SuspendToken(Foo foo) => _foo = foo;

        public void Dispose()
        {
            if (_foo != null) _foo._suspendCount--;
            _foo = null; // nulling the reference IS the disposed guard
        }
    }
}
```

Notice the disposal guard here. Instead of a separate `bool _disposed` flag, nulling `_foo` serves double duty — it prevents the double-decrement and releases the reference to the parent in one move. The null-check before `_suspendCount--` ensures a second call to `Dispose()` is silently harmless.

The `_suspendCount` integer (rather than a simple bool) handles **nested suspensions** correctly. If two callers independently suspend events, the count goes to 2, and events only resume when both tokens are disposed and the count returns to 0:

```csharp
using var outer = foo.SuspendEvents(); // count = 1
    using var inner = foo.SuspendEvents(); // count = 2
    // inner disposes → count = 1, still suspended
// outer disposes → count = 0, events resume
```

A plain boolean flag would break the moment two suspensions overlapped.

---

## Stage 2 — The Generic `Disposable` Helper

Looking at `SuspendToken`, nothing about it is specific to `Foo`. It holds a reference and calls something on disposal. The same boilerplate would need to be written for every class that uses this pattern. That is a sign that the pattern itself deserves a reusable home.

The solution is a generic `Disposable` helper class that captures any `Action` and executes it on disposal:

```csharp
public class Disposable : IDisposable
{
    public static Disposable Create(Action onDispose) => new Disposable(onDispose);

    private Action _onDispose;

    private Disposable(Action onDispose) => _onDispose = onDispose;

    public void Dispose()
    {
        _onDispose?.Invoke(); // execute the action if not already disposed
        _onDispose = null;    // null it out — makes Dispose() idempotent for free
    }
}
```

The null-check on `_onDispose?.Invoke()` is the same idempotency trick used in `SuspendToken`. The second call to `Dispose()` finds null and does nothing. You get safe, repeatable disposal without a `bool _disposed` flag — the action reference itself is the guard.

With this helper in place, `SuspendEvents()` collapses to a single expression:

```csharp
public IDisposable SuspendEvents()
{
    _suspendCount++;
    return Disposable.Create(() => _suspendCount--);
}
```

The lambda captures `this` implicitly, so `_suspendCount--` refers to the correct instance. The entire `SuspendToken` class is gone. The intent is stated directly with no ceremony.

---

## The Full Picture

Here is the final, clean implementation of `Foo`:

```csharp
class Foo
{
    private int _suspendCount;

    public IDisposable SuspendEvents()
    {
        _suspendCount++;
        return Disposable.Create(() => _suspendCount--);
    }

    private void FireSomeEvent()
    {
        if (_suspendCount == 0)
            Console.WriteLine("Event fired!");
    }
}
```

And the call site:

```csharp
var foo = new Foo();

using (foo.SuspendEvents())
{
    foo.FireSomeEvent(); // suppressed
    foo.FireSomeEvent(); // suppressed
}

foo.FireSomeEvent(); // fires — "Event fired!"
```

Or with the modern `using var`:

```csharp
using var _ = foo.SuspendEvents();
foo.FireSomeEvent(); // suppressed until end of scope
```

---

## The Broader Pattern

`Disposable.Create()` is not limited to event suspension. Any paired action where the second half must always run can be expressed this way:

```csharp
// Temporarily change the cursor to a wait cursor
public IDisposable BusyCursor()
{
    Cursor.Current = Cursors.WaitCursor;
    return Disposable.Create(() => Cursor.Current = Cursors.Default);
}

// Temporarily suppress layout recalculations on a panel
public IDisposable SuspendLayout()
{
    panel.SuspendLayout();
    return Disposable.Create(() => panel.ResumeLayout());
}

// Temporarily redirect console output
public IDisposable RedirectConsole(TextWriter writer)
{
    var original = Console.Out;
    Console.SetOut(writer);
    return Disposable.Create(() => Console.SetOut(original));
}
```

Every one of these follows the same shape: perform an action, return a token, undo the action on dispose. The `using` statement turns into a **scoped operation** mechanism — a way to say "do this for exactly this block of code, then undo it, no matter what."

This is the **Anonymous Disposal Pattern** — using `IDisposable` not to free a resource, but to represent the lifetime of a temporary state change. It is one of the most practical and underused patterns in everyday C# code.
