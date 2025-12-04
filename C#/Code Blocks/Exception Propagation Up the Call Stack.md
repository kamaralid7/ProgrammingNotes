Here are clear, structured notes on **exception propagation up the call stack in C#**. These are good for study or review.

---

# **Exception Propagation Up the Call Stack (C#)**

## **1. What Is Exception Propagation?**

Exception propagation refers to how an exception moves **upward through the chain of method calls** when it is not handled in the method where it occurred.

If a method doesn’t catch an exception, the runtime passes it back to the method that called it, and so on, until:

* A matching `catch` block handles it, **or**
* It reaches the top-level handler and crashes the program.

---

## **2. How the Call Stack Works**

When methods call each other, they form a *stack*:

```
Main()
  → MethodA()
      → MethodB()
          → MethodC()  // exception occurs here
```

If `MethodC` throws an exception and doesn’t handle it:

* It propagates to `MethodB`
* If `MethodB` doesn’t handle it → goes to `MethodA`
* If `MethodA` doesn’t handle it → goes to `Main`
* If `Main` doesn’t handle it → runtime terminates program (or global exception handler)

---

## **3. Example**

```csharp
void MethodC()
{
    throw new InvalidOperationException("Something went wrong");
}

void MethodB()
{
    MethodC();
}

void MethodA()
{
    MethodB();
}

void Main()
{
    MethodA();
}
```

If only `Main` has a `try/catch`, it handles the exception:

```csharp
try
{
    MethodA();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

**Propagation path:**
`MethodC → MethodB → MethodA → Main → caught`

---

## **4. Stack Trace**

When an exception is thrown, the **stack trace** shows the path of propagation:

```
at MethodC()
at MethodB()
at MethodA()
at Main()
```

This is extremely useful for debugging.

---

## **5. Handling Exceptions at the Right Level**

General tips:

* **Handle exceptions only when you can do something meaningful**
  (e.g., retry, fallback, show message).
* **Let exceptions bubble up if the caller can better respond**.
* Avoid catching everything at every layer unless necessary.

---

## **6. Re-throwing Exceptions**

Sometimes you catch an exception but still want it to propagate further.

### **Correct rethrow (preserves stack trace):**

```csharp
catch
{
    throw;
}
```

### **Incorrect rethrow (resets stack trace):**

```csharp
catch (Exception ex)
{
    throw ex;  // loses original stack trace
}
```

---

## **7. Finally Block Still Runs**

Even when exceptions propagate up the call stack, `finally` always executes:

```csharp
try
{
    MethodThatFails();
}
finally
{
    Console.WriteLine("Runs even if exception propagates");
}
```

---

## **8. Common Patterns**

### **Top-Level Catch**

Often used in UI or service apps:

```csharp
try
{
    RunApplication();
}
catch (Exception ex)
{
    Log(ex);
    ShowError();
}
```

### **Exception Wrapping**

Useful for adding context:

```csharp
catch (SqlException ex)
{
    throw new DataAccessException("Failed to load orders", ex);
}
```

---

## **9. Unhandled Exception Handlers**

For last-resort catching:

* `AppDomain.CurrentDomain.UnhandledException`
* `TaskScheduler.UnobservedTaskException`
* `Application.ThreadException` (WinForms)

These prevent crashes in production but are not meant for normal logic.
