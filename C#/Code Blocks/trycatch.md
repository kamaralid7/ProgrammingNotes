## C# Exception Handling – Study Notes  

---

### 1. What is a try–catch block?

- A **try–catch** block is used to handle **runtime errors (exceptions)** gracefully so your program doesn’t crash.
- Code that may fail goes in `try`, and error-handling code goes in `catch`.

```csharp
try
{
    // Code that might throw an exception
}
catch (ExceptionType ex)
{
    // Code to handle the exception
}
```

---

### 2. Basic example

```csharp
try
{
    int x = int.Parse("abc"); // Throws FormatException
    Console.WriteLine(x);
}
catch (FormatException ex)
{
    Console.WriteLine("Input was not a valid number.");
}
```

- `int.Parse("abc")` throws a `FormatException`.
- The `catch (FormatException)` block handles it and prevents a crash.

---

### 3. Multiple catch blocks

You can handle **different exception types separately**:

```csharp
try
{
    int[] nums = { 1, 2 };
    Console.WriteLine(nums[5]); // IndexOutOfRangeException
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine("Index is out of bounds.");
}
catch (Exception ex)
{
    Console.WriteLine("Something went wrong.");
}
```

- C# checks catch blocks **top to bottom**.
- The **first matching catch** handles the exception; the rest are skipped.

---

### 4. try–catch–finally

Use `finally` for code that must **always** run, whether an exception happens or not (e.g., closing files, connections).

```csharp
try
{
    File.ReadAllText("data.txt");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine("File not found.");
}
finally
{
    Console.WriteLine("Finished attempting to read file.");
}
```

- `finally` runs:
  - if no exception occurs
  - if an exception is thrown and caught
  - even if an exception is thrown and not caught in this method

---

### 5. When to use try–catch

Use try–catch around code that is **likely to fail at runtime**, for example:

1. I/O operations  
   - File read/write  
   - Database operations  
2. Parsing user input  
3. Network calls (HTTP requests, sockets)  
4. Any external resource that may be unavailable or invalid  

---

### 6. Order of catch blocks – the key rule

#### 6.1 Top-to-bottom matching

- C# evaluates catch blocks **from top to bottom**.
- The **first compatible type** wins.
- Therefore, **more specific exceptions must come before more general ones**.

#### 6.2 Invalid ordering (compile-time error)

```csharp
try
{
    // Some code
}
catch (Exception ex)
{
    // General exception
}
catch (ArgumentNullException ex)   // ❌ Unreachable
{
    // More specific exception
}
```

- `ArgumentNullException` derives from `Exception`.
- `catch (Exception)` would already catch all `ArgumentNullException`s.
- Compiler error: the specific catch is **unreachable**.

#### 6.3 Correct ordering (Specific → General)

```csharp
try
{
    // Some code
}
catch (ArgumentNullException ex)
{
    Console.WriteLine("Argument was null.");
}
catch (Exception ex)
{
    Console.WriteLine("Some other exception occurred.");
}
```

- Works because the **specific** exception comes first.

---

### 7. Base vs. derived exception classes

Suppose you have an exception hierarchy:

```csharp
class MyBaseException : Exception { }
class MyDerivedException : MyBaseException { }
```

#### Incorrect order (will not compile)

```csharp
try
{
    throw new MyDerivedException();
}
catch (MyBaseException ex)
{
    Console.WriteLine("Caught base exception.");
}
catch (MyDerivedException ex)  // ❌ Unreachable
{
    Console.WriteLine("Caught derived exception.");
}
```

- `MyBaseException` will catch **both** `MyBaseException` and `MyDerivedException`.
- `catch (MyDerivedException)` is never reached → compiler error.

#### Correct order

```csharp
try
{
    throw new MyDerivedException();
}
catch (MyDerivedException ex)
{
    Console.WriteLine("Caught derived exception.");
}
catch (MyBaseException ex)
{
    Console.WriteLine("Caught base exception.");
}
```

**Output:**

```text
Caught derived exception.
```

---

### 8. How exception matching works

C# exception handling uses:

1. **Runtime type matching**
2. **Top-down catch evaluation**

Process:

1. An exception object is thrown (`throw new SomeException();`).
2. The runtime looks at its **actual type**.
3. It walks the catch blocks from **top to bottom**.
4. The **first catch** whose type is compatible (same type or base type) is executed.
5. Remaining catch blocks are **ignored**.

---

### 9. Application-layer example with custom exceptions

Assume 3 layers:

1. **BaseRepository** – throws `UserNotFoundException`
2. **ReadOnlyUserRepository** – throws `UserNotExistException`
3. **DBConnect** – throws generic `Exception`

Catch sequence:

```csharp
try
{
    // Call repositories / DB methods
}
catch (UserNotFoundException ex)      // 1 - more specific
{
    // Handle base repo user-not-found
}
catch (UserNotExistException ex)      // 2 - another specific case
{
    // Handle readonly repo user-not-exist
}
catch (Exception ex)                  // 3 - general fallback
{
    // Handle all other exceptions
}
```

#### Key points:

- Exception type, **not class/repository hierarchy**, decides which catch block runs.
- Example flows:
  1. `throw new UserNotExistException()` → caught by `catch (UserNotExistException ex)`
  2. `throw new UserNotFoundException()` → caught by `catch (UserNotFoundException ex)`
  3. `throw new Exception()` → caught by `catch (Exception ex)`

- Exception always **bubbles up the call stack**:
  `DBConnect → ReadOnlyUserRepository → BaseRepository → Caller`

---

### 10. Example of an invalid catch order with inheritance

Inheritance chain:

```text
Exception
  ↑
UserNotFoundException
  ↑
UserIdInvalid
```

Invalid order:

```csharp
try
{
    throw new UserIdInvalid();
}
catch (Exception ex)                // catches ALL exceptions
{
}
catch (UserNotFoundException ex)    // ❌ Unreachable
{
}
catch (UserIdInvalid ex)            // ❌ Unreachable
{
}
```

- Compiler error:
  - A previous catch clause catches all exceptions of this type or a super type.
- This code **will not compile**, so it can **never run**.

Correct order:

```csharp
try
{
    throw new UserIdInvalid();
}
catch (UserIdInvalid ex)            // most specific
{
}
catch (UserNotFoundException ex)    // less specific
{
}
catch (Exception ex)                // most general
{
}
```

---

### 11. Core rules to remember

1. **Order matters**: `Most specific → Most general`.
2. A `catch` for a **base exception** must always be **after** any catches for its **derived exceptions**.
3. The **first matching catch** handles the exception; others are ignored.
4. If a more general catch makes a more specific catch unreachable, the **compiler will error out**.
5. Use `finally` for clean-up code that must always run.

---

### 12. Quick summary table

| Concept                                      | Rule / Behavior                                 |
|---------------------------------------------|-------------------------------------------------|
| try–catch                                   | Handle exceptions without crashing the program  |
| Multiple catches                            | Checked **top to bottom**                       |
| Specific vs general exceptions              | **Specific first**, general last                |
| Base before derived in catch                | ❌ Compile-time error (unreachable code)        |
| Derived before base in catch                | ✔ Correct and required                          |
| Thrown `DerivedException`                   | Caught by first matching derived → base → `Exception` |
| `finally` block                             | Always runs (success, handled, or unhandled)    |

