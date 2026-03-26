# Advanced C# — 14: The Standard Event Pattern

> *"The official Microsoft convention. Once you know it, you see it everywhere in .NET."*

---

## The 5 Parts

```
1. EventArgs subclass     — carries the event data
2. Event declaration      — using EventHandler<T>
3. protected virtual OnXxx — the raiser method
4. Trigger                — calls OnXxx from within the class
5. Subscribers            — react via += from outside
```

---

## Part 1 — EventArgs Subclass

Always inherit from `EventArgs`. Use `init` properties — set once, read-only after:

```csharp
public class OrderPlacedEventArgs : EventArgs
{
    public int     OrderId  { get; init; }
    public decimal Amount   { get; init; }
    public string  Customer { get; init; } = "";
}
```

If you have no data to pass, use `EventArgs.Empty` — don't `new EventArgs()` each time.

> **Why subclass even when empty?** Adding data later is a breaking change if you used `EventArgs` directly. A named subclass lets you add properties in the future without breaking subscribers.

---

## Part 2 — Event Declaration

Always use `EventHandler<TEventArgs>` — never a raw `Action<T>` or custom delegate:

```csharp
public class OrderService
{
    // Standard signature: (object? sender, TEventArgs e)
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;
}
```

The `?` makes it nullable — no subscribers = null. Always nullable.

---

## Part 3 — The `protected virtual OnXxx` Raiser

The heart of the pattern:

```csharp
public class OrderService
{
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    // protected = subclasses can call it directly
    // virtual   = subclasses can override and intercept
    protected virtual void OnOrderPlaced(OrderPlacedEventArgs e)
    {
        OrderPlaced?.Invoke(this, e); // 'this' = the sender
    }
}
```

Why `protected virtual`?

- **`protected`** — subclasses can react to the event without subscribing to their own event
- **`virtual`** — subclasses can override to add behavior before/after the event fires

---

## Part 4 — Triggering the Event

Build args, then call `OnXxx`. Convention: raise **after** state changes:

```csharp
public class OrderService
{
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public void PlaceOrder(int orderId, decimal amount, string customer)
    {
        // Do the actual work first
        SaveToDatabase(orderId, amount);

        // Then raise the event
        OnOrderPlaced(new OrderPlacedEventArgs
        {
            OrderId  = orderId,
            Amount   = amount,
            Customer = customer
        });
    }

    protected virtual void OnOrderPlaced(OrderPlacedEventArgs e)
        => OrderPlaced?.Invoke(this, e);
}
```

---

## Part 5 — Subscribing

```csharp
var service = new OrderService();

service.OrderPlaced += (sender, e) =>
    Console.WriteLine($"Order #{e.OrderId} placed by {e.Customer} for ${e.Amount}");

service.OrderPlaced += (sender, e) =>
    EmailService.Send(e.Customer, "Your order is confirmed!");

service.PlaceOrder(101, 49.99m, "Ali");
// Order #101 placed by Ali for $49.99
// [email sent]
```

---

## The Subclass Override — Why `virtual` Matters

```csharp
public class LoggingOrderService : OrderService
{
    protected override void OnOrderPlaced(OrderPlacedEventArgs e)
    {
        Console.WriteLine($"[LOG] About to fire OrderPlaced for #{e.OrderId}");
        base.OnOrderPlaced(e); // fires the event + all subscribers
        Console.WriteLine($"[LOG] OrderPlaced fired");
    }
}
```

Without `virtual`, the subclass would have to subscribe to its own event — messy and slightly inefficient.

---

## Complete Example

```csharp
// EventArgs
public class StockAlertEventArgs : EventArgs
{
    public string  Symbol    { get; init; } = "";
    public decimal Price     { get; init; }
    public decimal Threshold { get; init; }
}

// Publisher
public class StockMonitor
{
    public event EventHandler<StockAlertEventArgs>? PriceThresholdBreached;

    protected virtual void OnPriceThresholdBreached(StockAlertEventArgs e)
        => PriceThresholdBreached?.Invoke(this, e);

    public void CheckPrice(string symbol, decimal price, decimal threshold)
    {
        if (price > threshold)
        {
            OnPriceThresholdBreached(new StockAlertEventArgs
            {
                Symbol    = symbol,
                Price     = price,
                Threshold = threshold
            });
        }
    }
}

// Subscribers
var monitor = new StockMonitor();

monitor.PriceThresholdBreached += (_, e) =>
    Console.WriteLine($"ALERT: {e.Symbol} hit ${e.Price} (threshold: ${e.Threshold})");

monitor.PriceThresholdBreached += (_, e) =>
    NotificationService.Push($"{e.Symbol} price alert!");

monitor.CheckPrice("AAPL", 210m, 200m);
// ALERT: AAPL hit $210 (threshold: $200)
// [push notification sent]
```

---

## Naming Conventions

| Thing | Convention | Example |
|-------|-----------|---------|
| EventArgs class | `[Name]EventArgs` | `OrderPlacedEventArgs` |
| Event name | Past tense verb / noun | `OrderPlaced`, `PriceChanged` |
| Raiser method | `On[EventName]` | `OnOrderPlaced` |
| Sender parameter | `sender` | `object? sender` |
| Args parameter | `e` | `OrderPlacedEventArgs e` |

> **Past tense** for events — `OrderPlaced` not `PlaceOrder`. Events describe things that *happened*, not things to *do*.

---

## The Checklist

```
✅ EventArgs subclass — even if empty today
✅ EventHandler<TEventArgs> — not Action<T> or custom delegate
✅ Nullable event — the ? is required
✅ protected virtual OnXxx — always, even if you think you won't subclass
✅ ?.Invoke(this, e) — null-safe, 'this' as sender
✅ Past-tense event name — OrderPlaced not OrderPlace
✅ Raise AFTER state changes — subscribers see the new state
```

---

## Variations — When to Bend the Rules

### No data — use EventArgs.Empty

```csharp
public event EventHandler? Started;

protected virtual void OnStarted()
    => Started?.Invoke(this, EventArgs.Empty); // not new EventArgs()
```

### Static events — rare but valid

```csharp
public class AppLifetime
{
    public static event EventHandler? Shutdown;

    public static void OnShutdown()
        => Shutdown?.Invoke(null, EventArgs.Empty); // sender = null for static
}
```

### Custom add/remove — for advanced control

```csharp
private EventHandler<OrderPlacedEventArgs>? _orderPlaced;
private readonly object _lock = new();

public event EventHandler<OrderPlacedEventArgs>? OrderPlaced
{
    add    { lock (_lock) { _orderPlaced += value; } }
    remove { lock (_lock) { _orderPlaced -= value; } }
}
```

---

## Summary

| Rule | Why |
|------|-----|
| `EventHandler<T>` always | Consistent, works with all .NET tools and patterns |
| `protected virtual OnXxx` | Subclasses can intercept or react without self-subscribing |
| `?.Invoke(this, e)` | Thread-safe null check + standard sender convention |
| Inherit `EventArgs` | Future-proof — add data later without breaking callers |
| Past-tense name | Events describe what happened, not what to do |
| Raise after state change | Subscribers see consistent, updated state |

---

*Previous: [13 - Events](./13%20-%20Events.md)*
*Next: See [toExplore.md](./toExplore.md) for what to tackle next!*
