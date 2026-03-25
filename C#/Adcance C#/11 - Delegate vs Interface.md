# Advanced C# — 11: Delegate vs Interface

> *"Both inject behavior. One is a contract for an object. The other is a contract for a single method."*

---

## The Core Difference

```
Interface = a CONTRACT for an object with multiple related behaviors  (noun)
Delegate  = a CONTRACT for a single method                           (verb)
```

When you think **noun** → interface. When you think **verb** → delegate.

---

## Same Problem, Two Solutions

```csharp
// ── Interface approach ──────────────────────────────────────────
public interface IPricingStrategy
{
    decimal Calculate(decimal basePrice);
}

public class DiscountStrategy : IPricingStrategy
{
    private readonly decimal _discount;
    public DiscountStrategy(decimal d) => _discount = d;
    public decimal Calculate(decimal price) => price * (1 - _discount);
}

var cart = new ShoppingCart(new DiscountStrategy(0.10m));

// ── Delegate approach ────────────────────────────────────────────
Func<decimal, decimal> discountStrategy = price => price * 0.90m;
Func<decimal, decimal> taxStrategy      = price => price * 1.15m;

var cart = new ShoppingCart(discountStrategy);
```

The delegate version is shorter — but that's not the whole story.

---

## When Delegate Wins 🏆

### 1. Single method — no need for a whole class

```csharp
// Interface forces boilerplate for one method
public interface IValidator { bool Validate(string input); }
public class EmailValidator : IValidator { ... }

// Delegate — clean and inline
Func<string, bool> validate = s => s.Contains("@") && s.Contains(".");
```

### 2. Composing and chaining behaviors

```csharp
Func<string, string> trim    = s => s.Trim();
Func<string, string> upper   = s => s.ToUpper();
Func<string, string> exclaim = s => s + "!";

var pipeline = trim.Then(upper).Then(exclaim);
// Interfaces can't compose like this without extra plumbing
```

### 3. Storing a list of callbacks

```csharp
// Natural with delegates
var onSave = new List<Action<Document>>();
onSave.Add(doc => Console.WriteLine("Saved"));
onSave.Add(doc => AuditLog.Write(doc));
onSave.Add(doc => NotifyUser(doc));

// Awkward with interfaces — needs List<IDocumentObserver> + classes
```

### 4. Lightweight — no heap object for the class

```csharp
button.Click += (s, e) => Console.WriteLine("Clicked"); // no class needed
```

---

## When Interface Wins 🏆

### 1. Multiple related methods that belong together

```csharp
// These form a coherent contract — interface is the right tool
public interface IRepository<T>
{
    T              GetById(int id);
    IEnumerable<T> GetAll();
    void           Save(T entity);
    void           Delete(int id);
}

// 4 separate delegates would be messy and error-prone
```

### 2. Implementer needs state and lifecycle

```csharp
public class DatabaseRepository : IRepository<Order>, IDisposable
{
    private readonly DbConnection _connection; // shared state
    public Order GetById(int id) { /* uses _connection */ }
    public void  Dispose()       { _connection.Dispose(); }
}
```

### 3. Dependency Injection

```csharp
// DI containers are built around interfaces
services.AddScoped<IEmailService, SmtpEmailService>();
services.AddScoped<IOrderRepository, SqlOrderRepository>();
```

### 4. Multiple swappable implementations

```csharp
ILogger logger = new FileLogger();
logger = new ConsoleLogger();
logger = new CloudLogger(); // all interchangeable
```

### 5. Async contracts

```csharp
public interface IEmailService
{
    Task      SendAsync(string to, string subject, string body);
    Task<bool> ValidateAddressAsync(string email);
}
// Func<string,string,string,Task> is noisy and unclear by comparison
```

---

## The One-Method Interface Signal

When an interface has exactly one method, that's a signal a delegate might be better:

```csharp
// These are all equivalent
public interface ITransformer  { string Transform(string input); }  // interface + class
Func<string, string> transform = s => s.ToUpper();                  // delegate — simpler
```

---

## They Can Work Together

Interfaces can *contain* delegate slots. Delegates can *call through* interface methods.

```csharp
// Interface with delegate extension points
public interface IDataPipeline
{
    void Process(
        IEnumerable<string> data,
        Func<string, bool>  filter,       // delegate slot
        Action<string>      onComplete    // delegate slot
    );
}

// Delegate that calls through an interface method
ILogger logger = new FileLogger();
Action<string> log = logger.Log; // captures interface method as a delegate
```

---

## Decision Flowchart

```
More than one related method?              → Interface
Implementer needs state / lifecycle?       → Interface
Going into a DI container?                 → Interface
Multiple swappable implementations?        → Interface

Single behavior, possibly inline?          → Delegate
Composing or chaining behaviors?           → Delegate
Callback or event handler?                 → Delegate
Short-lived, no class needed?              → Delegate
```

---

## Summary

| | Interface | Delegate |
|--|-----------|----------|
| Methods | Multiple related | Single |
| State | Naturally supports | Via closure (gets messy) |
| DI | Standard | Unusual |
| Composition | Requires plumbing | Natural |
| Overhead | New class per impl | Lambda inline |
| Async | Clean contracts | Noisy signatures |
| Thinking style | **Noun** — "be something" | **Verb** — "do something" |

---

*Previous: [10 - Func and Action with ref, out and Pointers](./10%20-%20Func%20and%20Action%20with%20ref%2C%20out%20and%20Pointers.md)*
*Next: [12 - Delegate Compatibility](./12%20-%20Delegate%20Compatibility.md)*
