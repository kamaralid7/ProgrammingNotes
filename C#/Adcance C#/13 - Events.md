# Advanced C# — 13: Events

> *"One object says something happened. Everyone who cares reacts. Nobody needs to know about anybody else."*

---

## What Problem Do Events Solve?

Without events, objects know about each other directly — tight coupling:

```csharp
// BAD — Button knows about every possible listener
public class Button
{
    public Logger    logger;
    public Analytics analytics;
    public UI        ui;

    public void Click()
    {
        logger.Log("clicked");       // knows about Logger
        analytics.Track("clicked");  // knows about Analytics
        ui.Highlight();              // knows about UI
    }
}
```

With events, `Button` knows about *nobody* — others subscribe themselves:

```csharp
// GOOD — Button knows nothing about its listeners
public class Button
{
    public event EventHandler? Clicked;

    public void SimulateClick()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
```

---

## Anatomy of an Event

```csharp
public class Button
{
    // 1. Declare the event
    public event EventHandler? Clicked;

    // 2. Protected virtual raiser
    protected virtual void OnClicked()
    {
        Clicked?.Invoke(this, EventArgs.Empty); // null-safe — fires only if subscribers exist
    }

    // 3. Something that triggers it
    public void SimulateClick() => OnClicked();
}

// 4. Subscribe
var btn = new Button();
btn.Clicked += (sender, e) => Console.WriteLine("Handler 1 fired");
btn.Clicked += (sender, e) => Console.WriteLine("Handler 2 fired");

// 5. Trigger
btn.SimulateClick();
// Handler 1 fired
// Handler 2 fired
```

---

## Custom EventArgs — Passing Data With Events

```csharp
// Step 1: define your data bag
public class PriceChangedEventArgs : EventArgs
{
    public decimal OldPrice { get; init; }
    public decimal NewPrice { get; init; }
    public decimal Change   => NewPrice - OldPrice;
}

// Step 2: declare the event with your EventArgs
public class Product
{
    private decimal _price;

    public decimal Price
    {
        get => _price;
        set
        {
            if (value == _price) return;

            var args = new PriceChangedEventArgs
            {
                OldPrice = _price,
                NewPrice = value
            };
            _price = value;
            OnPriceChanged(args); // raise AFTER updating state
        }
    }

    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    protected virtual void OnPriceChanged(PriceChangedEventArgs e)
        => PriceChanged?.Invoke(this, e);
}

// Step 3: subscribe and react
var product = new Product { Price = 100m };

product.PriceChanged += (sender, e) =>
    Console.WriteLine($"Price: ${e.OldPrice} → ${e.NewPrice} ({e.Change:+0.00;-0.00})");

product.Price = 89.99m;  // Price: $100 → $89.99 (-10.01)
product.Price = 120m;    // Price: $89.99 → $120 (+30.01)
```

---

## event vs Plain Delegate — The Key Distinction

`event` adds **encapsulation** on top of a delegate:

```csharp
public class Publisher
{
    public event  Action? Safe;    // event
    public        Action? Unsafe;  // plain delegate
}

var pub = new Publisher();

// Both allow subscribe and unsubscribe
pub.Safe   += () => Console.WriteLine("A");
pub.Unsafe += () => Console.WriteLine("A");

// Plain delegate — DANGER from outside
pub.Unsafe  = () => Console.WriteLine("Wiped everything!"); // ✅ compiles — wipes all subscribers
pub.Unsafe?.Invoke();                                        // ✅ invoke from outside

// Event — protected from outside
pub.Safe  = () => Console.WriteLine("Wiped!");  // ❌ compile error
pub.Safe?.Invoke();                              // ❌ compile error
```

`event` enforces: only the **declaring class** can wipe or fire it. Subscribers can only `+=` and `-=`.

---

## Cancellable Events — Let Subscribers Veto

```csharp
public class FileDeleteEventArgs : CancelEventArgs // has built-in Cancel property
{
    public string FileName { get; init; } = "";
}

public class FileManager
{
    public event EventHandler<FileDeleteEventArgs>? Deleting;

    public bool Delete(string fileName)
    {
        var args = new FileDeleteEventArgs { FileName = fileName };
        Deleting?.Invoke(this, args);

        if (args.Cancel)
        {
            Console.WriteLine($"Deletion of {fileName} was cancelled.");
            return false;
        }

        Console.WriteLine($"Deleted {fileName}.");
        return true;
    }
}

var fm = new FileManager();

fm.Deleting += (sender, e) =>
{
    if (e.FileName.EndsWith(".sys"))
    {
        Console.WriteLine($"Blocked: {e.FileName} is a system file!");
        e.Cancel = true;
    }
};

fm.Delete("document.txt"); // Deleted document.txt.
fm.Delete("kernel32.sys"); // Blocked! → Deletion cancelled.
```

---

## The `protected virtual OnXxx` Convention

Exists for two reasons:

**1. Subclasses can intercept how the event is raised:**

```csharp
public class LoggingButton : Button
{
    protected override void OnClicked()
    {
        Console.WriteLine("LoggingButton: about to fire Clicked");
        base.OnClicked(); // raises the event
        Console.WriteLine("LoggingButton: Clicked was fired");
    }
}
```

**2. Subclasses can react without subscribing to their own event:**

```csharp
protected override void OnClicked()
{
    // handle in subclass directly — more efficient than self-subscribing
    base.OnClicked(); // still lets external subscribers run
}
```

---

## Thread-Safe Event Invocation

```csharp
// UNSAFE — race condition between null check and invoke
if (Clicked != null)
    Clicked(this, EventArgs.Empty); // Clicked could be null by now!

// SAFE — capture local copy first
var handler = Clicked;              // atomic read
handler?.Invoke(this, EventArgs.Empty);

// MODERN — C# 6+ null conditional is also safe
Clicked?.Invoke(this, EventArgs.Empty); // compiler generates safe capture internally
```

---

## Memory Leak — The Most Common Event Bug

Events hold **strong references** to subscribers. Publisher outliving subscriber = memory leak:

```csharp
public class LiveDataFeed
{
    public event Action<decimal>? PriceUpdated;
}

public class PriceWidget
{
    public PriceWidget(LiveDataFeed feed)
    {
        feed.PriceUpdated += UpdateDisplay; // feed holds strong ref to this widget
    }
    private void UpdateDisplay(decimal price) { }
}

var feed   = new LiveDataFeed(); // lives for the whole app
var widget = new PriceWidget(feed);

widget = null; // ❌ NOT collected — feed still holds the reference!

// FIX: always unsubscribe when done
feed.PriceUpdated -= widget.UpdateDisplay;
```

---

## Events Are Just Syntactic Sugar

Under the hood, `event` compiles to a private delegate field + add/remove accessors:

```csharp
// What you write
public event EventHandler? Clicked;

// What the compiler generates (roughly)
private EventHandler? _clicked;

public event EventHandler? Clicked
{
    add    { _clicked += value; }
    remove { _clicked -= value; }
}
```

You can write custom `add`/`remove` for advanced scenarios like thread-safe storage or weak references.

---

## Real-World: Order Processing Lifecycle

```csharp
public class OrderProcessor
{
    public event EventHandler<OrderEventArgs>? OrderReceived;
    public event EventHandler<OrderEventArgs>? OrderValidated;
    public event EventHandler<OrderEventArgs>? OrderShipped;
    public event EventHandler<OrderEventArgs>? OrderFailed;

    public void Process(Order order)
    {
        OnOrderReceived(order);

        if (!Validate(order))
        {
            OnOrderFailed(order, "Validation failed");
            return;
        }

        OnOrderValidated(order);
        Ship(order);
        OnOrderShipped(order);
    }
}

// Wire up behavior WITHOUT touching OrderProcessor
var processor = new OrderProcessor();
processor.OrderReceived  += (_, e) => Console.WriteLine($"Received  #{e.Order.Id}");
processor.OrderValidated += (_, e) => Console.WriteLine($"Validated #{e.Order.Id}");
processor.OrderShipped   += (_, e) => Console.WriteLine($"Shipped   #{e.Order.Id}");
processor.OrderFailed    += (_, e) => Console.WriteLine($"FAILED    #{e.Order.Id}: {e.Reason}");
```

---

## Summary

| Concept | Key Point |
|---------|-----------|
| `event` keyword | Encapsulates a delegate — outside code can only `+=` / `-=` |
| `EventHandler<T>` | Standard delegate type for events |
| Custom `EventArgs` | Carries data — inherit from `EventArgs`, use `init` |
| `?.Invoke(this, e)` | Always null-safe, always pass `this` as sender |
| `protected virtual OnXxx` | Raiser — subclasses can override or call directly |
| Cancellable events | Inherit `CancelEventArgs`, check `e.Cancel` after invoke |
| Memory leak | Unsubscribe when subscriber is done — or it never gets collected |
| Compiler output | Private delegate field + add/remove accessors |

---

*Previous: [12 - Delegate Compatibility](./12%20-%20Delegate%20Compatibility.md)*
*Next: [14 - Standard Event Pattern](./14%20-%20Standard%20Event%20Pattern.md)*
