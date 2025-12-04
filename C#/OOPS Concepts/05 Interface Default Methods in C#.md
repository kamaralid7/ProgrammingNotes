Starting from **C# 8**, interfaces in C# can contain **default method implementations**.
This is a major shift because before C# 8, an interface could only contain **method signatures** without bodies.

Let’s break this concept down clearly:

---

# ⭐ 1. **What Is a Default Interface Method?**

A *default interface method* is a method **inside an interface** that has a **method body**.

### Example:

```csharp
public interface ILogger
{
    void Log(string message); // abstract method (must implement)

    // Default implementation added in C# 8
    void LogInfo(string message)
    {
        Console.WriteLine("[Info] " + message);
    }
}
```

Before C# 8, this would NOT compile.
Now it's fully allowed.

---

# ⭐ 2. **Why Were Default Interface Methods Introduced?**

To solve the problem of **breaking changes**.

If you add a new method to an interface like:

```csharp
void LogWarning(string message);
```

Every class implementing that interface must update → this breaks existing code.

To avoid that, you now can define:

```csharp
void LogWarning(string message)
{
    Console.WriteLine("[Warning] " + message);
}
```

So the interface provides fallback behavior.

---

# ⭐ 3. **Example: Interface With Default Methods**

```csharp
public interface ICalculator
{
    int Add(int a, int b);

    // Default implementation
    int Multiply(int a, int b)
    {
        return a * b;
    }
}
```

### Implementing class:

```csharp
public class SimpleCalculator : ICalculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

### Usage:

```csharp
var calc = new SimpleCalculator();

Console.WriteLine(calc.Add(2, 3));       // 5
Console.WriteLine(calc.Multiply(2, 3)); // 6 (from interface default method)
```

Notice that **SimpleCalculator** does NOT implement `Multiply()`, but still inherits it from the interface.

---

# ⭐ 4. **Implementing Class Can Override the Default Method**

```csharp
public class AdvancedCalculator : ICalculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Multiply(int a, int b)  // Overrides interface default
    {
        Console.WriteLine("Custom multiply");
        return a * b;
    }
}
```

### Usage:

```csharp
ICalculator calc = new AdvancedCalculator();
calc.Multiply(3, 4);
```

Output:

```
Custom multiply
```

---

# ⭐ 5. **Important Rule: Default Interface Methods Are Not Used for Multiple Inheritance Resolution**

This will *not* cause ambiguity:

```csharp
public interface IA
{
    void Hello()
    {
        Console.WriteLine("Hello from IA");
    }
}

public interface IB
{
    void Hello()
    {
        Console.WriteLine("Hello from IB");
    }
}

public class Test : IA, IB
{
    // Must resolve the conflict!
    void IA.Hello() => Console.WriteLine("Hello from IA implemented");
    void IB.Hello() => Console.WriteLine("Hello from IB implemented");
}
```

### Usage:

```csharp
IA objA = new Test();
objA.Hello();

IB objB = new Test();
objB.Hello();
```

---

# ⭐ 6. **Explicit Usage of Default Methods**

A class can call other methods inside an interface’s default implementation:

```csharp
public interface IPrinter
{
    void Print(string msg);

    void PrintLine(string msg)
    {
        Print(msg + "\n"); // Calls abstract method Print()
    }
}
```

### Implementing class:

```csharp
public class ConsolePrinter : IPrinter
{
    public void Print(string msg)
    {
        Console.Write(msg);
    }
}
```

### Usage:

```csharp
IPrinter printer = new ConsolePrinter();
printer.PrintLine("Hello"); 
```

Output:

```
Hello
```

---

# ⭐ 7. **Why Not Use Default Interface Methods Everywhere?**

Because they are NOT full class implementations.

**Limitations:**

| Feature               | Allowed in Interfaces? |
| --------------------- | ---------------------- |
| Fields                | ❌ No                   |
| Constructors          | ❌ No                   |
| Protected members     | ❌ No                   |
| Private instance data | ❌ No                   |

So default methods are mostly for:

* Backward compatibility
* Shared helper methods
* Optional behaviors

Not full object models.

---

# ⭐ 8. **Real-World Scenario: Logging**

### Interface with default method:

```csharp
public interface ILogger
{
    void Log(string message);

    void LogInfo(string message)
    {
        Log("[INFO] " + message);
    }

    void LogError(string message)
    {
        Log("[ERROR] " + message);
    }
}
```

### Implementing class:

```csharp
public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}
```

### Usage:

```csharp
ILogger logger = new ConsoleLogger();
logger.LogInfo("App started");
logger.LogError("Something went wrong");
```

---

# 🎉 SUMMARY (Easy to Remember)

### ✔ Before C# 8

Interfaces = only method signatures → **no bodies allowed**

### ✔ After C# 8

Interfaces can contain:

* Default methods
* Static methods
* Private methods (for internal reuse)

### ✔ Benefits

* Prevent breaking changes
* Support interface evolution
* Provide optional behavior

### ✔ Still NOT a replacement for abstract classes

Because interfaces cannot have:

* Fields
* Constructors
* Object state

---

If you'd like, I can now explain:

🔥 Default method + multiple inheritance conflict resolution
🔥 Difference between **abstract class default methods** vs **interface default methods**
🔥 How the CLR (Common Language Runtime) actually invokes interface default methods
🔥 Interview questions on default interface methods

Would you like those?
