Generics in **C#** are a powerful feature that allows you to define classes, interfaces, methods, and delegates with **type parameters** — meaning you can write code that works with **any data type** while still maintaining **type safety** and **performance**.

---

### 🔹 Why Use Generics?

1. **Type Safety** – Generics allow compile-time type checking, preventing type-related errors and reducing runtime exceptions.
2. **Reusability & Flexibility** – Generic classes and methods can work with any data type, reducing code duplication and improving maintainability.
3. **Performance Improvement** – Generics eliminate the need for boxing and unboxing, leading to faster execution and better memory efficiency.
4. **Cleaner Code** – Less casting and type-checking makes code easier to read, understand, and maintain.
5. **Integration with LINQ** – Generics enable type-safe queries and seamless use with LINQ for data manipulation.

Without generics, you’d often use `object` types for flexibility, but that:

* Loses **compile-time type safety**
* Requires **boxing/unboxing** for value types (hurts performance)
* Involves **type casting**, which can cause runtime errors

Generics solve all of this by letting you write reusable, type-safe code.

---

### 🔹 Basic Example

```csharp
// Generic class
public class Box<T>
{
    private T value;

    public void SetValue(T v)
    {
        value = v;
    }

    public T GetValue()
    {
        return value;
    }
}

// Usage
Box<int> intBox = new Box<int>();
intBox.SetValue(10);
Console.WriteLine(intBox.GetValue());  // Output: 10

Box<string> strBox = new Box<string>();
strBox.SetValue("Hello Generics");
Console.WriteLine(strBox.GetValue());  // Output: Hello Generics
```

Here, `T` is a **type parameter** — it can be replaced with any type when you instantiate the class.

---

### 🔹 Generic Methods

You can also create **generic methods** inside non-generic or generic classes:

```csharp
public class Utilities
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }
}

// Usage
int x = 5, y = 10;
Utilities.Swap(ref x, ref y);
Console.WriteLine($"{x}, {y}"); // Output: 10, 5
```

---

### 🔹 Generic Interfaces

```csharp
public interface IRepository<T>
{
    void Add(T item);
    T Get(int id);
}

public class UserRepository : IRepository<User>
{
    public void Add(User user) { /* ... */ }
    public User Get(int id) { return new User(); }
}
```

---

### 🔹 Generic Constraints (`where` clause)

You can restrict what kinds of types can be used for `T`:

| Constraint             | Meaning                                          |
| ---------------------- | ------------------------------------------------ |
| `where T : struct`     | `T` must be a value type                         |
| `where T : class`      | `T` must be a reference type                     |
| `where T : new()`      | `T` must have a public parameterless constructor |
| `where T : BaseClass`  | `T` must derive from `BaseClass`                 |
| `where T : IInterface` | `T` must implement `IInterface`                  |
| `where T1 : T2`        | `T1` must be or derive from `T2`                 |

Example:

```csharp
public class Factory<T> where T : new()
{
    public T CreateInstance()
    {
        return new T();
    }
}
```

---

### 🔹 Built-in Generic Collections

C# provides many generic collections in `System.Collections.Generic`:

* `List<T>`
* `Dictionary<TKey, TValue>`
* `Queue<T>`
* `Stack<T>`
* `HashSet<T>`

Example:

```csharp
List<string> names = new List<string>();
names.Add("Alice");
names.Add("Bob");

foreach (var name in names)
    Console.WriteLine(name);
```

---

### 🔹 Advantages of Generics

✅ **Type Safety** – errors are caught at compile time
✅ **Reusability** – one definition works for any type
✅ **Performance** – no boxing/unboxing
✅ **Cleaner Code** – reduces duplication

---

### 🔹 Summary

| Feature                | Example                               |
| ---------------------- | ------------------------------------- |
| Generic Class          | `class Box<T> { }`                    |
| Generic Method         | `void Print<T>(T item)`               |
| Generic Interface      | `interface IRepository<T>`            |
| Constraint             | `where T : class, new()`              |
| Built-in Generic Types | `List<T>`, `Dictionary<TKey, TValue>` |

---

Would you like me to show a **real-world example**, like a **generic repository pattern** or a **custom generic data structure** (e.g., stack or linked list)?
