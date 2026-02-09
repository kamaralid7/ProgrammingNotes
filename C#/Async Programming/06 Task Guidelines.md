

## The guideline

> **Do not block on async code.**
> Blocking (`Wait`, `Result`, `GetAwaiter().GetResult()`) + async = potential **deadlock**.

---

## Why does deadlock happen at all?

Because **async code often wants to “come back” to the same thread that is now blocked**.

That’s it. That’s the core bug.

---

## The key player: `SynchronizationContext`

Some environments have a **single-threaded context**:

* UI apps (WPF, WinForms)
* Old ASP.NET (not ASP.NET Core)

This context says:

> “Continuations must resume on *this* thread.”

---

## The deadly sequence (step by step)

### 1️⃣ You are on the UI thread

```csharp
public void Button_Click()
{
    var result = GetDataAsync().Result; // ❌ blocking
}
```

UI thread is now **blocked** waiting.

---

### 2️⃣ Async method starts

```csharp
async Task<string> GetDataAsync()
{
    await Task.Delay(1000);
    return "Done";
}
```

* `Task.Delay` completes on a thread pool thread
* `await` captures the **UI SynchronizationContext**

Meaning:

> “When I continue, I must go back to the UI thread.”

---

### 3️⃣ Deadlock 💀

* UI thread is blocked waiting for `.Result`
* Continuation wants the UI thread
* UI thread can’t run continuation
* Continuation can’t finish task
* Task never completes

🔒 **Perfect circular wait**

---

## Visual model (remember this)

```
UI Thread:
┌─────────────┐
│ .Result     │ ← waiting
│   🔒        │
└─────────────┘
        ▲
        │ continuation wants this thread
        │
Async continuation
```

---

## Why `GetAwaiter().GetResult()` is also bad

```csharp
GetDataAsync().GetAwaiter().GetResult();
```

* Avoids `AggregateException`
* ❌ Still blocks the thread
* ❌ Still deadlocks

So it’s **cleaner syntax, same poison** ☠️

---

## Why this does NOT happen everywhere

### ASP.NET Core

* No `SynchronizationContext`
* Continuations run on thread pool
* Deadlock unlikely

### Console apps

* No sync context
* Usually safe (but still bad style)

⚠️ But library code **doesn’t know** where it’s called from.

---

## That’s why the rule exists

> **Library code must never block on async code.**

Because:

* Your library might be used in UI
* Or legacy ASP.NET
* Or a single-threaded scheduler

---

## Proper solution #1 (BEST): async all the way

```csharp
async void Button_Click()
{
    var result = await GetDataAsync();
}
```

⬆ No blocking
⬆ No deadlock
⬆ Natural exception flow

---

## Proper solution #2: `ConfigureAwait(false)` (library code)

```csharp
async Task<string> GetDataAsync()
{
    await Task.Delay(1000).ConfigureAwait(false);
    return "Done";
}
```

Now:

* Continuation does **not** require UI thread
* `.Result` will not deadlock

⚠️ But:

* UI updates must marshal back manually
* Consumers still shouldn’t block

---

## Why `ConfigureAwait(false)` is not a silver bullet

* Easy to forget
* Breaks UI access
* Only applies to awaited calls inside that method
* Doesn’t fix callers that block

---

## Why blocking is tempting (and why to resist it)

### Temptation

```csharp
var data = GetDataAsync().Result;
```

* “I just need the value”
* “This is easier”
* “It works on my machine”

### Reality

* UI freeze
* Random deadlocks
* Production-only bugs
* Thread pool starvation

---

## Official guideline (summarized)

✔ Prefer `await`
✔ Make callers async
✔ Propagate async upward
❌ Avoid `.Wait()`, `.Result()`
❌ Avoid `.GetAwaiter().GetResult()`
❌ Never block in libraries

---

## One-line rule to tattoo in your brain

> **Async code wants to finish on a thread — don’t block that thread.**

---

## When is blocking *acceptable*?

Very rare cases:

* Application entry points (`Main` in old .NET)
* Legacy sync APIs at the boundary
* Fully thread-pool-only environments (with extreme care)

Even then:

```csharp
Task.Run(async () => await FooAsync()).GetAwaiter().GetResult();
```

Still a compromise, not a victory.

---

If you want, next we can:



Say the word 🔥
