# When to Dispose — and When Not To

## The Core Principle

If a type implements `IDisposable`, that's the author's way of saying: *"I'm holding onto something the runtime can't automatically clean up — please tell me when you're done."*

The rule is simple on the surface:

> **If you created it and you own it, dispose it.**

But real code isn't always that clean. There are legitimate scenarios where disposing is wrong, unnecessary, or even harmful. Understanding those exceptions is just as important as understanding the rule itself.

---

## Why Disposable Types Usually Touch Unmanaged Resources

Most types implement `IDisposable` because they hold an **unmanaged handle** — directly or indirectly. Unmanaged handles are the gateway to the outside world: OS file locks, network sockets, database connections, window handles, GDI objects. These are the primary ways an object can cause real-world trouble if not cleaned up. The .NET runtime has no visibility into them, so it cannot free them on your behalf — you must.

Even if a type doesn't hold an unmanaged handle directly, it might hold one indirectly by wrapping another `IDisposable`, which in turn wraps one. The chain leads back to a handle somewhere.

---

## When You Should Dispose

### WinForms Controls

WinForms controls (buttons, labels, panels, forms) all implement `IDisposable` because they wrap a **Win32 window handle (HWND)** — a genuine unmanaged OS resource. If you create controls dynamically at runtime, they must be disposed.

```csharp
// Controls added to a form are disposed automatically when the form is disposed
var button = new Button { Text = "Click me" };
this.Controls.Add(button);
// When the Form is disposed, it disposes all its child controls

// But if you create a control and never add it to a form, you must dispose it yourself
var orphanLabel = new Label { Text = "Never shown" };
orphanLabel.Dispose(); // essential — it holds an HWND
```

When a `Form` is disposed (which happens automatically when you close it), it walks its entire control tree and calls `Dispose()` on every child. This is the ownership chain working at scale.

### GDI+ Objects — Pens, Brushes, Bitmaps, Fonts

GDI+ objects are classic examples of types that must always be disposed. Each one wraps an unmanaged GDI handle managed by Windows. Failing to dispose them causes **GDI handle leaks**, which accumulate silently until Windows refuses to create new graphics objects — a hard limit of around 10,000 GDI handles per process.

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    // Each of these allocates an unmanaged GDI handle
    using var pen    = new Pen(Color.Red, 2);
    using var brush  = new SolidBrush(Color.Blue);
    using var bitmap = new Bitmap(100, 100);

    e.Graphics.DrawRectangle(pen, 10, 10, 80, 80);
    e.Graphics.FillRectangle(brush, 20, 20, 60, 60);
    e.Graphics.DrawImage(bitmap, Point.Empty);

} // All three disposed here — GDI handles released immediately
```

`OnPaint` can be called dozens of times per second during resizing or animation. Without `using`, GDI handles pile up faster than the GC can finalise them, and your application crashes or stops rendering.

`Font` behaves the same way:

```csharp
using var font = new Font("Arial", 14, FontStyle.Bold);
e.Graphics.DrawString("Hello", font, Brushes.Black, 10, 10);
// font disposed at end of scope
```

---

## When NOT to Dispose

There are three well-defined scenarios where disposing is the wrong thing to do.

### Scenario 1 — You Don't Own the Object

If you obtained an object from somewhere else — a static field, a shared property, a singleton — you did not create it, and you do not own it. Disposing it would pull the rug out from under every other caller that's still using it.

**`System.Drawing` static brushes and pens** are the canonical example. The `Brushes` and `Pens` classes expose pre-created GDI+ objects as static properties. These are shared, cached objects managed by the framework:

```csharp
// These are shared instances — DO NOT dispose them
e.Graphics.FillRectangle(Brushes.Blue, rect);   // Brushes.Blue is a cached, shared object
e.Graphics.DrawLine(Pens.Red, p1, p2);          // Pens.Red is equally shared

// Contrast with objects YOU created — these you must dispose
using var myBrush = new SolidBrush(Color.Blue); // you own this
using var myPen   = new Pen(Color.Red, 2);      // you own this
```

Similarly, `Font.FromHdc()` returns a font derived from an existing device context handle. You own the `Font` object returned, so you must dispose it. But the HDC itself belongs to the caller of the device context — you must not dispose that.

The distinguishing question is always: **who created this?** If you called `new`, you own it. If you got it from a static property or a factory that manages its own lifecycle, you don't.

### Scenario 2 — Disposing Would Do Something You Don't Want

Some types use `Dispose()` to perform an action that is harmful or undesirable in certain contexts. The most important examples are in `System.IO` and `System.Data`.

**`StreamReader` / `StreamWriter` wrapping a shared stream:**

```csharp
// You want to write to a MemoryStream in multiple passes
var ms = new MemoryStream();

using (var writer = new StreamWriter(ms))
{
    writer.Write("Hello");
} // StreamWriter.Dispose() closes the underlying MemoryStream — ms is now unusable!

ms.Position = 0; // ObjectDisposedException — ms was disposed by the writer
```

`StreamWriter.Dispose()` closes the underlying stream. If that stream belongs to you and you still need it, disposing the writer is harmful. The fix is `leaveOpen: true`:

```csharp
var ms = new MemoryStream();

using (var writer = new StreamWriter(ms, leaveOpen: true))
{
    writer.Write("Hello");
} // StreamWriter is disposed, but ms is left open

ms.Position = 0;
var result = new StreamReader(ms).ReadToEnd(); // works fine
```

**`IDbConnection` / `DbContext` in shared or injected contexts:**

In dependency injection containers (ASP.NET Core, for example), database connections and `DbContext` instances are often managed by the container with a defined lifetime (scoped, transient, singleton). If you dispose them manually inside a method, you break the container's lifecycle management:

```csharp
// BAD — you don't own this, the DI container does
public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context; // injected — you did NOT create this
    }

    public void DoWork()
    {
        var users = _context.Users.ToList();
        _context.Dispose(); // WRONG — disposing something you don't own
        // The container will now try to use a disposed context
    }
}
```

The rule: if it was injected, the container owns it and will dispose it at the right time. Don't touch it.

### Scenario 3 — Disposing Is Unnecessary by Design

Some types implement `IDisposable` as a formality — their `Dispose()` method does nothing meaningful, or they operate entirely in managed memory with no unmanaged handles at all. Disposing them adds noise to your code without any real benefit.

**`StringReader` and `StringWriter`** are the textbook examples. They read from and write to a plain `string` or `StringBuilder` in memory — no files, no handles, no OS resources. Their `Dispose()` methods exist only because they inherit from `TextReader`/`TextWriter`, which implement `IDisposable` for the sake of their subclasses (`StreamReader`, `StreamWriter`) that genuinely need it.

```csharp
// Technically fine to use without using — Dispose() does nothing real
var reader = new StringReader("hello world");
var line = reader.ReadLine();
// No resource leak here — there's nothing to leak

// The using is harmless but adds visual clutter for no benefit
using var reader2 = new StringReader("hello world"); // unnecessary, but not wrong
```

**`MemoryStream`** is a similar case. Its `Dispose()` marks it as disposed but doesn't actually release the backing byte array — that's managed memory which the GC handles. However, the convention in most teams is to still use `using` with `MemoryStream` for consistency and to prevent accidental use after the intended scope.

**`IDBConnection`** is a similar case.

**`DBContect(EFCore)`** is a similar case.
---

## Summary

| Scenario | Example | Should you dispose? |
|----------|---------|-------------------|
| You created it with `new` | `new Pen(...)`, `new FileStream(...)` | **Yes — always** |
| WinForms controls you added dynamically | `new Button()` outside a form | **Yes** |
| GDI+ objects (Pen, Brush, Bitmap, Font) | `new SolidBrush(...)` | **Yes** |
| Shared static GDI+ objects | `Brushes.Blue`, `Pens.Red` | **No — you don't own them** |
| Injected dependencies (DI container) | `DbContext` via constructor injection | **No — the container owns it** |
| Stream wrapping a stream you still need | `StreamWriter` over a `MemoryStream` | **Use `leaveOpen: true`** |
| Purely in-memory types | `StringReader`, `StringWriter` | **No — Dispose() is a no-op** |

The unifying question behind every row in that table is the same: **do you own it?** Ownership is what creates the obligation to dispose.
